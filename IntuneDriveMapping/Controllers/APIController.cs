using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntuneDriveMapping.Models;

namespace IntuneDriveMapping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly IntuneDriveMappingContext _context;

        public APIController(IntuneDriveMappingContext context)
        {
            _context = context;
        }


        // GET: api/API
        [HttpGet]
        public IEnumerable<DriveMappingModel> GetDriveMappingModel()
        {
            DriveMappingModel test = new DriveMappingModel
            {
                Path="\\gugus",
                DriveLetter="X",
                Id=2
            };
            yield return test;
            //Test
        }

        // GET: api/API/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriveMappingModel([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var driveMappingModel = await _context.DriveMappingModel.FindAsync(id);

            if (driveMappingModel == null)
            {
                return NotFound();
            }

            return Ok(driveMappingModel);
        }

        // PUT: api/API/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDriveMappingModel([FromRoute] int id, [FromBody] DriveMappingModel driveMappingModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != driveMappingModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(driveMappingModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriveMappingModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/API
        [HttpPost]
        public async Task<IActionResult> PostDriveMappingModel([FromBody] DriveMappingModel driveMappingModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.DriveMappingModel.Add(driveMappingModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDriveMappingModel", new { id = driveMappingModel.Id }, driveMappingModel);
        }

        // DELETE: api/API/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriveMappingModel([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var driveMappingModel = await _context.DriveMappingModel.FindAsync(id);
            if (driveMappingModel == null)
            {
                return NotFound();
            }

            _context.DriveMappingModel.Remove(driveMappingModel);
            await _context.SaveChangesAsync();

            return Ok(driveMappingModel);
        }

        private bool DriveMappingModelExists(int id)
        {
            return _context.DriveMappingModel.Any(e => e.Id == id);
        }
    }
}