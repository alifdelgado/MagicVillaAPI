using MagicVilla_API.Logging;
using MagicVillaAPI.Data;
using MagicVillaAPI.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController:ControllerBase
    {
        private readonly ILogging _logger;

        public VillaAPIController(ILogging logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            _logger.Log("Getting all villas", "info");
            return Ok(VillaStore.villaList);
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if(id == 0) {
                _logger.Log("GetVilla error with Id: " + id, "error");
                return BadRequest("Invalid Id");
            }
            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            if(villa == null) {
                _logger.Log("GetVilla error with Id: " + id, "error");
                return NotFound();
            }
            _logger.Log("Returning Villa with Id: " + id, "info");
            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            if(VillaStore.villaList.FirstOrDefault(v => v.Name.ToLower() == villaDTO.Name.ToLower()) != null) {
                _logger.Log("Villa already exists!", "error");
                ModelState.AddModelError("CustomError", "Villa Already Exists!");
                return BadRequest(ModelState);

            }
            if(villaDTO == null) {
                _logger.Log("Villa is null!", "error");
                return BadRequest(villaDTO);
            }
            if(villaDTO.Id > 0) {
                _logger.Log("Villa id should be zero!", "error");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            villaDTO.Id = VillaStore.villaList.OrderByDescending(v => v.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(villaDTO);
            _logger.Log("Villa created successfully!", "info");
            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id}, villaDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteVilla(int id)
        {
            if(id == 0) {
                _logger.Log("DeleteVilla error with Id: " + id, "error");
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            if(villa == null) {
                _logger.Log("DeleteVilla error with Id: " + id, "error");
                return NotFound();
            }
            VillaStore.villaList.Remove(villa);
            _logger.Log("Villa deleted successfully!", "info");
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO)
        {
            if (villaDTO == null || id != villaDTO.Id) {
                _logger.Log("UpdateVilla error with Id: " + id, "error");
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            if (villa == null) {
                _logger.Log("UpdateVilla error with Id: " + id, "error");
                return NotFound();
            }
            villa.Name = villaDTO.Name;
            villa.Sqft = villaDTO.Sqft;
            villa.Occupancy = villaDTO.Occupancy;
            _logger.Log("Villa updated successfully!", "info");
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "PartialUpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PartialUpdateVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
        {
            if (patchDTO == null || id == 0) {
                _logger.Log("PartialUpdateVilla error with Id: " + id, "error");
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            if (villa == null) {
                _logger.Log("PartialUpdateVilla error with Id: " + id, "error");
                return NotFound();
            }
            patchDTO.ApplyTo(villa, error => ModelState.AddModelError("PatchError", error.ErrorMessage));
            if (!ModelState.IsValid) {
                _logger.Log("PartialUpdateVilla error with Id: " + id, "error");
                return BadRequest(ModelState);
            }
            _logger.Log("Villa updated successfully!", "info");
            return NoContent();
        }
    }
}