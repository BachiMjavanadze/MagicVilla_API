using System.Collections.Generic;
using System.Reflection;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace MagicVilla_VillaAPI.Controllers;
//[Route("api/[controller]")]
[Route("api/VillaAPI")]
[ApiController]
public class VillaAPIController : ControllerBase
{
  private readonly ApplicationDbContext _db;
  private readonly IVillaRepository _dbVilla;
  private readonly IMapper _mapper;

  public VillaAPIController(
      ApplicationDbContext db,
      IMapper mapper,
      IVillaRepository dbVilla
    )
  {
    //_db = db;
    _mapper = mapper;
    _dbVilla = dbVilla;
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
  {
    //IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
    IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
    IEnumerable<VillaDTO> villaDtoList = _mapper.Map<IEnumerable<VillaDTO>>(villaList);
    return Ok(villaDtoList);
  }

  [HttpGet("{id:int}", Name = "GetVilla")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<VillaDTO>> GetVilla(int id)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    if (id < 0)
    {
      return BadRequest();
    }

    //Villa? villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
    Villa? villa = await _dbVilla.GetAsync(v => v.Id == id);
    VillaDTO villaDto = _mapper.Map<VillaDTO>(villa);

    if (villa == null)
    {
      return NotFound();
    }

    return Ok(villaDto);
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO createDTO)
  {
    //if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
    if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
    {
      ModelState.AddModelError("CustomeError", "Villa already exists!");
      return BadRequest();
    }

    if (createDTO == null)
    {
      return BadRequest(createDTO);
    }

    Villa model = _mapper.Map<Villa>(createDTO);
    //await _db.Villas.AddAsync(model);
    //await _db.SaveChangesAsync();

    await _dbVilla.CreateAsync(model);

    return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
  }

  [HttpDelete("{id:int}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteVilla(int id)
  {
    if (id <= 0)
    {
      return BadRequest();
    }

    //Villa? villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
    Villa? villa = await _dbVilla.GetAsync(v => v.Id == id);

    if (villa == null)
    {
      return NotFound();
    }

    //_db.Villas.Remove(villa);
    //await _db.SaveChangesAsync();
    await _dbVilla.RemoveAsync(villa);

    return NoContent();
  }

  [HttpPut("id:int")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO UpdateDTO)
  {
    //if (UpdateDTO == null || id != UpdateDTO.Id)
    //{
    //  return BadRequest();
    //}

    Villa model = _mapper.Map<Villa>(UpdateDTO);
    model.Id = id;

    //_db.Villas.Update(model);
    //await _db.SaveChangesAsync();
    await _dbVilla.UpdateAsync(model);

    return NoContent();
  }

  [HttpPatch("id:int", Name = "UpdatePartialVilla")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> UpdatePartialVilla(int id, [FromBody] JsonPatchDocument<VillaUpdateDTO> patchDTO)
  {
    if (patchDTO == null || id == 0)
    {
      return BadRequest();
    }

    //Villa? villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
    Villa? villa = await _dbVilla.GetAsync(v => v.Id == id, tracked: false);

    VillaUpdateDTO villaDto = _mapper.Map<VillaUpdateDTO>(villa);

    if (villa == null)
    {
      return BadRequest();
    }

    // save updated info from user into villaDto
    patchDTO.ApplyTo(villaDto, ModelState);
    Villa model = _mapper.Map<Villa>(villaDto);

    //_db.Villas.Update(model);
    //await _db.SaveChangesAsync();
    await _dbVilla.UpdateAsync(model);

    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    return NoContent();
  }
}
