using AutoMapper;
using ExpenseTrackerApi.Data;
using ExpenseTrackerApi.DTOs;
using ExpenseTrackerApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTrackerApi.Controllers;

[Authorize]
[ApiController]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public ExpensesController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenses()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var expenses = await _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var result = _mapper.Map<List<ExpenseResponseDto>>(expenses);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddExpense([FromBody] ExpenseRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var category = await _db.Categories
            .FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.Name.Trim().ToLowerInvariant() == dto.CategoryName.Trim().ToLowerInvariant());

        if (category == null)
        {
            category = new Category
            {
                Name = dto.CategoryName.Trim(),
                UserId = userId
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        var expense = _mapper.Map<Expense>(dto);
        expense.UserId = userId;
        expense.CategoryId = category.Id;

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        // Load category for DTO mapping
        await _db.Entry(expense).Reference(e => e.Category).LoadAsync();

        var result = _mapper.Map<ExpenseResponseDto>(expense);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseRequestDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var expense = await _db.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (expense == null)
            return NotFound();

        // Find category by name
        var category = await _db.Categories
            .FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.Name.Trim().ToLowerInvariant() == dto.CategoryName.Trim().ToLowerInvariant());

        if (category == null)
        {
            return BadRequest("Category not found. Please create the category first.");
        }

        expense.Title = dto.Title;
        expense.Amount = dto.Amount;
        expense.Date = dto.Date;
        expense.CategoryId = category.Id;

        await _db.SaveChangesAsync();

        // Reload category for updated DTO
        await _db.Entry(expense).Reference(e => e.Category).LoadAsync();

        var result = _mapper.Map<ExpenseResponseDto>(expense);
        return Ok(result);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (expense == null)
            return NotFound();

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
