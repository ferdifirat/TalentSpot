﻿using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services
{
    public interface IForbiddenWordsService
    {
      Task SetForbiddenWordsAsync(IEnumerable<string> words);

        Task<List<string>> GetForbiddenWordsAsync();
    }
}