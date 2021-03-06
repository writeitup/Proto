﻿using System.Linq;
using WriteItUp.Areas.Reviewer.Models;
using Raven.Client.Indexes;

namespace WriteItUp.Areas.Reviewer.Indexes
{
    public class PastReviewIndex : AbstractIndexCreationTask<PastReviewView>
    {
        public PastReviewIndex()
        {
            Map = docs => from review in docs
                select new { PublishDate = review.PublishDate, OwnerUserId = review.OwnerUserId, NickName = "Sally" };
        }
    }
}