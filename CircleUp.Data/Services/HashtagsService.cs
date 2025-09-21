using CircleUp.Data.Helpers;
using CircleUp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Services
{
    public class HashtagsService : IHashtagsService
    {
        private readonly AppDbContext _context;

        public HashtagsService(AppDbContext context)
        {
                _context = context;
        }
        public async Task ProcessHashtagsForNewPostAsync(string content)
        {
            var postHashtags = HashtagHelper.GetHashtags(content);
            foreach (var hashTag in postHashtags)
            {
                var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(n => n.Name == hashTag);
                if (hashtagDb != null)
                {
                    hashtagDb.Count += 1;
                    hashtagDb.DateUpdated = DateTime.Now;

                    _context.Hashtags.Update(hashtagDb);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var newHashtag = new Hashtag()
                    {
                        Name = hashTag,
                        Count = 1,
                        DateCreated = DateTime.Now,
                        DateUpdated = DateTime.Now,

                    };
                    await _context.Hashtags.AddAsync(newHashtag);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task ProcessHashtagsForRemovePostAsync(string content)
        {

            //Update hashtags
            var postHashtags = HashtagHelper.GetHashtags(content);
            foreach (var hashtag in postHashtags)
            {
                var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(n => n.Name == hashtag);
                if (hashtagDb != null)
                {
                    hashtagDb.Count -= 1;
                    hashtagDb.DateUpdated = DateTime.UtcNow;

                    _context.Hashtags.Update(hashtagDb);
                    await _context.SaveChangesAsync();
                }

            }
        }
    }
}
