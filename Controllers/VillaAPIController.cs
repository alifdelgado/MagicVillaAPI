using AutoMapper;
using MagicVilla_API.Data;
using MagicVillaAPI.Models;
using MagicVillaAPI.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController:ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public VillaAPIController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            IEnumerable<Villa> villaList = await _dbContext.Villas.ToListAsync();
            return Ok(_mapper.Map<List<VillaDTO>>(villaList));
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDTO>> GetVilla(int id)
        {
            if(id == 0) {
                return BadRequest("Invalid Id");
            }
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if(villa == null) {
                return NotFound();
            }
            return Ok(_mapper.Map<VillaDTO>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody]VillaCreateDTO villaDTO)
        {
            if(await _dbContext.Villas.FirstOrDefaultAsync(v => v.Name.ToLower() == villaDTO.Name.ToLower()) != null) {
                ModelState.AddModelError("CustomError", "Villa Already Exists!");
                return BadRequest(ModelState);

            }
            if(villaDTO == null) {
                return BadRequest(villaDTO);
            }
            Villa model = _mapper.Map<Villa>(villaDTO);
            await _dbContext.Villas.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            return CreatedAtRoute("GetVilla", new { id = model.Id}, model);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteVilla(int id)
        {
            if(id == 0) {
                return BadRequest();
            }
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if(villa == null) {
                return NotFound();
            }
            _dbContext.Villas.Remove(villa);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaDTO)
        {
            if (villaDTO == null || id != villaDTO.Id) {
                return BadRequest();
            }
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if (villa == null) {
                return NotFound();
            }
            Villa model = _mapper.Map<Villa>(villaDTO);
            _dbContext.Villas.Update(model);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "PartialUpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PartialUpdateVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0) {
                return BadRequest();
            }
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);
            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
            if (villa == null) {
                return NotFound();
            }
            patchDTO.ApplyTo(villaDTO, error => ModelState.AddModelError("PatchError", error.ErrorMessage));
            Villa model = _mapper.Map<Villa>(villaDTO);
            _dbContext.Villas.Update(model);
            await _dbContext.SaveChangesAsync();
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            return NoContent();
        }
    }
}