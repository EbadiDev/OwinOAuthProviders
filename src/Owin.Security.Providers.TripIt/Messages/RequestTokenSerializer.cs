﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Owin.Security.DataHandler.Serializer;

namespace Owin.Security.Providers.TripIt.Messages
{
    /// <summary>
    /// Serializes and deserializes TripIt request and access tokens so that they can be used by other application components.
    /// </summary>
    public class RequestTokenSerializer : IDataSerializer<RequestToken>
    {
        private const int FormatVersion = 1;

        /// <summary>
        /// Serialize a request token
        /// </summary>
        /// <param name="model">The token to serialize</param>
        /// <returns>A byte array containing the serialized token</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public virtual byte[] Serialize(RequestToken model)
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memory))
                {
                    Write(writer, model);
                    writer.Flush();
                    return memory.ToArray();
                }
            }
        }

        /// <summary>
        /// Deserializes a request token
        /// </summary>
        /// <param name="data">A byte array containing the serialized token</param>
        /// <returns>The TripIt request token</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public virtual RequestToken Deserialize(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(memory))
                {
                    return Read(reader);
                }
            }
        }

        /// <summary>
        /// Writes a TripIt request token as a series of bytes. Used by the <see cref="Serialize"/> method.
        /// </summary>
        /// <param name="writer">The writer to use in writing the token</param>
        /// <param name="token">The token to write</param>
        public static void Write(BinaryWriter writer, RequestToken token)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            writer.Write(FormatVersion);
            writer.Write(token.Token);
            writer.Write(token.TokenSecret);
            writer.Write(token.CallbackConfirmed);
            PropertiesSerializer.Write(writer, token.Properties);
        }

        /// <summary>
        /// Reads a TripIt request token from a series of bytes. Used by the <see cref="Deserialize"/> method.
        /// </summary>
        /// <param name="reader">The reader to use in reading the token bytes</param>
        /// <returns>The token</returns>
        public static RequestToken Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.ReadInt32() != FormatVersion)
            {
                return null;
            }

            var token = reader.ReadString();
            var tokenSecret = reader.ReadString();
            var callbackConfirmed = reader.ReadBoolean();
            var properties = PropertiesSerializer.Read(reader);
            if (properties == null)
            {
                return null;
            }

            return new RequestToken { Token = token, TokenSecret = tokenSecret, CallbackConfirmed = callbackConfirmed, Properties = properties };
        }
    }
}