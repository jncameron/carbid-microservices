using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

        // var auctions = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
        // return _mapper.Map<List<AuctionDto>>(auctions);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null) return NotFound();
        
        return _mapper.Map<AuctionDto>(auction);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto auctionDto)
    {
        // Validate the auctionDto object.

        // Map the auctionDto object to an Auction entity.
        var auction = _mapper.Map<Auction>(auctionDto);

        // Save the auction entity to the database.
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;

        // Return a response.
        if (!result) return BadRequest("Could not save changes to DB");
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        // Validate the auctionDto object.

        // Map the auctionDto object to an Auction entity.
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();
        //TODO Check that seller == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.ImageUrl = updateAuctionDto.ImageUrl ?? auction.Item.ImageUrl;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        

        var result = await _context.SaveChangesAsync() > 0;
        if (result) return Ok();
        // Return a response.
        return BadRequest("Could not save changes to DB");
        
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null) return NotFound();
        
        _context.Auctions.Remove(auction);
        
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not remove auction from DB");
        return Ok();

    }
}