
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OllamaSharp;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Connect to the local Ollama model
        IChatClient chatClient = new OllamaApiClient(
            new Uri("http://localhost:11434"),
            "llama3.2:1b"
        );

        // Store conversation history in memory
        List<ChatMessage> chatHistory = new List<ChatMessage>
        {
            new ChatMessage(
                ChatRole.System,
                """
                You are a friendly and helpful hiking assistant.

                Help users discover hiking trails and enjoy nature.

                First, ask the user for their location and preferred
                hiking difficulty.

                Recommend three suitable hiking trails with different
                distances or difficulty levels.

                Share an interesting nature fact about the area.

                Do not invent trail locations, distances, or conditions.
                Remind users to check official trail information before
                visiting and to hike with appropriate supervision.
                """
            )
        };

        // Create a file to store conversations
        string historyFile = "chat-history.txt";

        Console.WriteLine("================================");
        Console.WriteLine("       ChatAppAI");
        Console.WriteLine("================================");
        Console.WriteLine("Using local AI: llama3.2:1b");
        Console.WriteLine("Conversation history will be saved.");
        Console.WriteLine("Type 'exit' to close the application.");

        // Start the conversation
        while (true)
        {
            Console.WriteLine();
            Console.Write("You: ");

            string? userPrompt = Console.ReadLine();

            // Ignore empty messages
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                continue;
            }

            // Exit the application
            if (userPrompt.Equals(
                "exit",
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            // Add the user's message to conversation history
            chatHistory.Add(
                new ChatMessage(ChatRole.User, userPrompt)
            );

            // Save the user's message
            await File.AppendAllTextAsync(
                historyFile,
                $"You: {userPrompt}{Environment.NewLine}"
            );

            Console.WriteLine();
            Console.Write("AI: ");

            string response = "";

            // Get a streaming response from Ollama
            await foreach (
                ChatResponseUpdate update in
                chatClient.GetStreamingResponseAsync(chatHistory))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    Console.Write(update.Text);
                    response += update.Text;
                }
            }

            // Add the AI response to conversation history
            chatHistory.Add(
                new ChatMessage(ChatRole.Assistant, response)
            );

            // Save the AI response
            await File.AppendAllTextAsync(
                historyFile,
                $"AI: {response}{Environment.NewLine}{Environment.NewLine}"
            );

            Console.WriteLine();
        }
    }
}
