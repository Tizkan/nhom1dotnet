using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        _context.Rooms.Add(Room);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}