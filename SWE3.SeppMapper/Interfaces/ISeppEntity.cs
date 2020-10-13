using System;

namespace SWE3.SeppMapper.Interfaces
{
    public interface ISeppEntity
    {
        int ID { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}