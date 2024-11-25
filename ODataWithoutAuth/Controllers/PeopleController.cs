using AirVinyl.API.DbContexts;
using AirVinyl.Entities;
using AirVinyl.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;


namespace AirVinyl.Controllers
{
    public class PeopleController : ODataController
    {
        private readonly AirVinylDbContext _airVinylDbContext;

        public PeopleController(AirVinylDbContext airVinylDbContext)
        {
            _airVinylDbContext = airVinylDbContext
                ?? throw new ArgumentNullException(nameof(airVinylDbContext));
        }


        [HttpGet("odata/People({key})")]
        // People(1)
        public async Task<IActionResult> Get(int key)
        {
            var person = await _airVinylDbContext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        [HttpGet("odata/People({key})/Email")]
        [HttpGet("odata/People({key})/FirstName")]
        [HttpGet("odata/People({key})/LastName")]
        [HttpGet("odata/People({key})/DateOfBirth")]
        [HttpGet("odata/People({key})/Gender")]
        public async Task<IActionResult> GetPersonProperty(int key)
        {
            var person = await _airVinylDbContext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            var propertyToGet = new Uri(HttpContext.Request.GetEncodedUrl()).Segments.Last();

            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);

            if (propertyValue == null)
            {
                // null = no content
                return NoContent();
            }

            return Ok(propertyValue);
        }


        [HttpGet("odata/People({key})/Email/$value")]
        [HttpGet("odata/People({key})/FirstName/$value")]
        [HttpGet("odata/People({key})/LastName/$value")]
        [HttpGet("odata/People({key})/DateOfBirth/$value")]
        [HttpGet("odata/People({key})/Gender/$value")]
        public async Task<IActionResult> GetPersonPropertyRawValue(int key)
        {
            var person = await _airVinylDbContext.People
              .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            var url = HttpContext.Request.GetEncodedUrl();
            var propertyToGet = new Uri(url).Segments[^2].TrimEnd('/');

            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);

            if (propertyValue == null)
            {
                // null = no content
                return NoContent();
            }

            return Ok(propertyValue.ToString());
        }

        // odata/People(key)/VinylRecords

        [HttpGet("odata/People({key})/VinylRecords")]
        public async Task<IActionResult> GetPersonCollectionProperty(int key)
        {
            var collectionPopertyToGet = new Uri(HttpContext.Request.GetEncodedUrl())
                .Segments.Last();

            var person = await _airVinylDbContext.People
                  .Include(collectionPopertyToGet)
                  .FirstOrDefaultAsync(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            if (!person.HasProperty(collectionPopertyToGet))
            {
                return NotFound();
            }

            return Ok(person.GetValue(collectionPopertyToGet));
        }

        // Mainpulating

        [HttpPost("odata/People")]
        public async Task<IActionResult> CreatePerson([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _airVinylDbContext.People .Add(person);
            await _airVinylDbContext.SaveChangesAsync();

            return Created(person);
        }

        [HttpPut("odata/People({key})")]
        public async Task<IActionResult> UpdatePerson(int key,[FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = await _airVinylDbContext.People.FirstOrDefaultAsync(p => p.PersonId == key); 
            if (currentPerson == null) 
            {
                return NotFound();
            }

            person.PersonId = currentPerson.PersonId;

            _airVinylDbContext.Entry(currentPerson).CurrentValues.SetValues(person);
            await _airVinylDbContext.SaveChangesAsync();

            return NoContent();
        }
        [HttpPatch("odata/People({key})")] 
        public async Task<IActionResult> PertialUpdatePerson (int key,[FromBody] Delta <Person> patch)
        {
            if (!ModelState.IsValid)
            {
            return BadRequest(ModelState); 
            }

            var currentPerson = await _airVinylDbContext.People.FirstOrDefaultAsync (p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound(ModelState);
            }

            patch.Patch(currentPerson);
            await _airVinylDbContext.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("odata/People")]
        [HttpDelete("odata/People({key})")]
        public async Task<IActionResult> DeletePerson (int key)
        {
            var currentPersion = await _airVinylDbContext.People.FirstOrDefaultAsync(c=>c.PersonId == key);
            if (currentPersion == null)
            {
                return NotFound(ModelState);
            }

            _airVinylDbContext.Remove(currentPersion);
            await _airVinylDbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
