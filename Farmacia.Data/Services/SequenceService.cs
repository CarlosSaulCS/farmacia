using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Data.Services;

public class SequenceService : ISequenceService
{
    private readonly PharmacyDbContext _context;

    public SequenceService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetNextFolioAsync(string sequenceName, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.Sequences.FirstOrDefaultAsync(s => s.Name == sequenceName, cancellationToken);
        if (sequence is null)
        {
            sequence = new Sequence
            {
                Name = sequenceName,
                Prefix = string.Empty,
                CurrentValue = 0,
                Padding = 6
            };
            _context.Sequences.Add(sequence);
        }

        sequence.CurrentValue += 1;

        await _context.SaveChangesAsync(cancellationToken);

        var formattedNumber = sequence.CurrentValue.ToString(new string('0', Math.Max(sequence.Padding, 1)));
        return string.Concat(sequence.Prefix, formattedNumber);
    }
}
