using Microsoft.EntityFrameworkCore;
using NetNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetNote.Repository
{
    public class NoteRepository : INoteRepository
    {
        private NoteContext context;
        public NoteRepository(NoteContext _context)
        {
            context = _context;
        }

        public Task AddAsync(Note note)
        {
            context.Notes.Add(note);
            return context.SaveChangesAsync();
        }

        public Task<Note> GetByIdAsync(int id)
        {
            //return context.Notes.FirstOrDefaultAsync(a => a.Id == id);
            return context.Notes.Include(type => type.Type).FirstOrDefaultAsync(a => a.Id == id);
        }

        public Task<List<Note>> ListAsync()
        {
            //return context.Notes.ToListAsync();
            return context.Notes.Include(type => type.Type).ToListAsync();
        }

        public Task UpdateAsync(Note note)
        {
            context.Entry(note).State = EntityState.Modified;
            return context.SaveChangesAsync();
        }

        public Tuple<List<Note>, int> PageList(int pageindex, int pagesize)
        {
            var query = context.Notes.Include(type => type.Type).AsQueryable();
            var count = query.Count();
            var pagecount = count % pagesize == 0 ? count / pagesize : count / pagesize + 1;
            var notes = query.OrderByDescending(r => r.Create)
                .Skip((pageindex - 1) * pagesize)
                .Take(pagesize)
                .ToList();
            return new Tuple<List<Note>, int>(notes, pagecount);
        }
    }
}
