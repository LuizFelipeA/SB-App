// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.Json;
using Microsoft.Azure.ServiceBus;
using SBShared.Models;

Console.WriteLine("Hello, World!");

const string connectionString = "";
const string queueName = "personqueue";
IQueueClient queueClient;

queueClient = new QueueClient(connectionString, queueName); // Connecting to the queue

var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
{
    MaxConcurrentCalls = 1, // Proccess 1 message per time.
    AutoComplete = false // Will not mark the message as complete / Wait until the message is succeffully read.
};

// Method that will gonna be call when new messages come in (listener)
queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

Console.ReadLine();

await queueClient.CloseAsync(); // Close connection with azure service bus queue

async Task ProcessMessagesAsync(Message message, CancellationToken cancellationToken)
{
    var jsonString = Encoding.UTF8.GetString(message.Body);

    var person = JsonSerializer.Deserialize<PersonModel>(jsonString);

    Console.WriteLine($"Person received: { person?.FirstName } { person?.LastName }");

    // Completing the message 'cause its not autoCompleted
    await queueClient.CompleteAsync(message.SystemProperties.LockToken);
}

Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
{
    Console.WriteLine($"Message handler exception: { arg.Exception }");
    return Task.CompletedTask;
}