using ClientesCFA.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesCFA.Models;
using System.Text.RegularExpressions;

namespace ClientesCFA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PersonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Person
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var people = await _context.People
                .AsNoTracking()
                .Include(p => p.Addresses)
                .Include(p => p.Phones)
                .Include(p => p.Emails)
                .ToListAsync();

            if (people == null || !people.Any())
                return NotFound("No people found.");

            return Ok(people);
        }

        // GET: api/Person/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var person = await _context.People
                .AsNoTracking()
                .Include(p => p.Addresses)
                .Include(p => p.Phones)
                .Include(p => p.Emails)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (person == null)
                return NotFound($"Person with ID {id} not found.");

            return Ok(person);
        }

        // POST: api/Person
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Person person)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!person.IsValidDocumentTypeForAge(out var errorMessage))
                return BadRequest(errorMessage);

            var existingPerson = await _context.People
                .AnyAsync(p => p.DocumentType == person.DocumentType && p.DocumentNumber == person.DocumentNumber);

            if (existingPerson)
                return Conflict("A person with the same document type and number already exists.");

            await _context.People.AddAsync(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = person.Id }, person);
        }

        // PUT: api/Person/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Person person)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingPerson = await _context.People
                .Include(p => p.Addresses)
                .Include(p => p.Phones)
                .Include(p => p.Emails)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (existingPerson == null)
                return NotFound($"Person with ID {id} not found.");

            existingPerson.DocumentType = person.DocumentType;
            existingPerson.DocumentNumber = person.DocumentNumber;
            existingPerson.Names = person.Names;
            existingPerson.LastName1 = person.LastName1;
            existingPerson.LastName2 = person.LastName2;
            existingPerson.Gender = person.Gender;
            existingPerson.BirthDate = person.BirthDate;

            _context.People.Update(existingPerson);
            await _context.SaveChangesAsync();

            return Ok(existingPerson);
        }

        // DELETE: api/Person/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var personExists = await _context.People.AnyAsync(p => p.Id == id);
            if (!personExists)
                return NotFound($"Person with ID {id} not found.");

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC DeletePersonById @PersonId = {0}", id);

            return NoContent();
        }

        // GET: api/Person/search/by-name?name=""
        [HttpGet("search/by-name")]
        public async Task<IActionResult> SearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("The name parameter is required.");

            var results = await (from person in _context.People
                                 where person.Names.Contains(name) ||
                                       person.LastName1.Contains(name) ||
                                       person.LastName2.Contains(name)
                                 orderby person.Names ascending
                                 select new
                                 {
                                     person.DocumentNumber,
                                     FullName = $"{person.Names} {person.LastName1} {person.LastName2}"
                                 }).ToListAsync();

            if (!results.Any())
                return NotFound("No clients found with the given name.");

            return Ok(results);
        }

        // GET: api/Person/search/by-document?documentNumber=""
        [HttpGet("search/by-document")]
        public async Task<IActionResult> SearchByDocument(string documentNumber)
        {

            if (!Regex.IsMatch(documentNumber, @"^[0-9]+$"))
                return BadRequest("The document number can only contain numbers.");

            var results = await _context.People
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.DocumentNumber, $"%{documentNumber}%"))
                .OrderByDescending(p => Convert.ToInt32(p.DocumentNumber))
                .Select(p => new
                {
                    p.DocumentNumber,
                    FullName = $"{p.Names} {p.LastName1} {p.LastName2}"
                })
                .ToListAsync();

            if (!results.Any())
                return NotFound("No clients found with the given document number.");

            return Ok(results);
        }

        // GET: api/Person/search/by-birthdate?startDate="2000-01-18"&endDate="2024-01-10"
        [HttpGet("search/by-birthdate")]
        public async Task<IActionResult> SearchByBirthDate(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("Start date cannot be after end date.");

            var results = await _context.People
                .AsNoTracking()
                .Where(p => p.BirthDate >= startDate && p.BirthDate <= endDate)
                .OrderBy(p => p.BirthDate)
                .Select(p => new
                {
                    p.BirthDate,
                    FullName = $"{p.Names} {p.LastName1} {p.LastName2}"
                })
                .ToListAsync();

            if (!results.Any())
                return NotFound("No clients found within the given date range.");

            return Ok(results);
        }

        // GET: api/Person/search/multiple-phones"
        [HttpGet("search/multiple-phones")]
        public async Task<IActionResult> SearchWithMultiplePhones()
        {
            var results = await _context.People
                .AsNoTracking()
                .Where(p => p.Phones.Count > 1)
                .Select(p => new
                {
                    FullName = $"{p.Names} {p.LastName1} {p.LastName2}",
                    PhoneCount = p.Phones.Count
                })
                .ToListAsync();

            if (!results.Any())
                return NotFound("No clients found with multiple phone numbers.");

            return Ok(results);
        }

        // GET: api/Person/search/multiple-addresses"
        [HttpGet("search/multiple-addresses")]
        public async Task<IActionResult> SearchWithMultipleAddresses()
        {
            var results = await _context.People
                .AsNoTracking()
                .Where(p => p.Addresses.Count > 1)
                .Select(p => new
                {
                    FirstAddress = p.Addresses.OrderBy(a => a.Id).FirstOrDefault().AddressLine,
                    FullName = $"{p.Names} {p.LastName1} {p.LastName2}"
                })
                .ToListAsync();

            if (!results.Any())
                return NotFound("No clients found with multiple addresses.");

            return Ok(results);
        }
    }
}
