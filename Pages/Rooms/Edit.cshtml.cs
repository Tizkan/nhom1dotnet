using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; }

    public async Task OnGetAsync(int id)
    {
        Room = await _context.Rooms.FindAsync(id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _context.Rooms.Update(Room);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}