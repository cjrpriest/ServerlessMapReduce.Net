using System;
using System.IO;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace ServerlessMapReduceDotNet.LambdaEntryPoints
{
    public class CommandSerialiser : ILambdaSerializer
    {
        private readonly JsonSerializer _serializer;

        public CommandSerialiser()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            int num = 3;
            serializerSettings.TypeNameHandling = (TypeNameHandling) num;
            this._serializer = JsonSerializer.Create(serializerSettings);
        }

        public T Deserialize<T>(Stream requestStream)
        {
            return this._serializer.Deserialize<T>((JsonReader) new JsonTextReader((TextReader) new StreamReader(requestStream)));
        }

        public void Serialize<T>(T response, Stream responseStream)
        {
            throw new NotImplementedException();
        }
    }
}