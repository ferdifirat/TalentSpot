using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalentSpot.Application.Constants
{
    public static class RedisCacheConstants
    {
        public const string ForbiddenWordCacheKey = "forbiddenwords";
        public const string WorkTypeCacheKey = "worktypes";
        public const string BenefitCacheKey = "benefits";
        // Add more keys as needed
    }
}
