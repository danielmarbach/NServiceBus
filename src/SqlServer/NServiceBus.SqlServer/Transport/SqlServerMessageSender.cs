﻿namespace NServiceBus.SQLServer.Transport
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Serializers.Json;
    using Unicast.Queuing;

    public class SqlServerMessageSender : ISendMessages
    {
        public string ConnectionString { get; set; }


        public void Send(TransportMessage message, Address address)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var sql = string.Format(SqlSend, address.Queue);
                connection.Open();
                using (var command = new SqlCommand(sql, connection) {CommandType = CommandType.Text})
                {
                    command.Parameters.Add("Id", SqlDbType.UniqueIdentifier).Value = Guid.Parse(message.Id);
                    command.Parameters.Add("CorrelationId", SqlDbType.VarChar).Value = GetValue(message.CorrelationId);
                    if (message.ReplyToAddress == null) // Sendonly endpoint
                        command.Parameters.AddWithValue("ReplyToAddress", string.Empty);
                    else
                        command.Parameters.AddWithValue("ReplyToAddress", message.ReplyToAddress.ToString());
                    command.Parameters.AddWithValue("Recoverable", message.Recoverable);
                    command.Parameters.AddWithValue("MessageIntent", message.MessageIntent.ToString());
                    command.Parameters.Add("TimeToBeReceived", SqlDbType.BigInt).Value = message.TimeToBeReceived.Ticks;
                    command.Parameters.AddWithValue("Headers", Serializer.SerializeObject(message.Headers));
                    command.Parameters.AddWithValue("Body", message.Body ?? new byte[0]);

                    command.ExecuteNonQuery();
                }
            }
        }

        static readonly JsonMessageSerializer Serializer = new JsonMessageSerializer(null);


        static object GetValue(object value)
        {
            return value ?? DBNull.Value;
        }

        const string SqlSend =
            @"INSERT INTO [{0}] ([Id],[CorrelationId],[ReplyToAddress],[Recoverable],[MessageIntent],[TimeToBeReceived],[Headers],[Body]) 
                                    VALUES (@Id,@CorrelationId,@ReplyToAddress,@Recoverable,@MessageIntent,@TimeToBeReceived,@Headers,@Body)";
    }
}