﻿using System.Linq;
using Proto2.Areas.Reviewer.Models;
using Raven.Client.Indexes;

namespace Proto2.Areas.Reviewer.Indexes
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