﻿using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Mappers
{
    public class MakeAccidentCountMapper : IMapperFunc
    {
        public KeyValuePairCollection Map(string line)
        {
            return new KeyValuePairCollection {
                new CountKvp
                {
                    Key = line.Split(',')[22], // make is 23rd column
                    Value = 1
                }
            };
        }
    }
}