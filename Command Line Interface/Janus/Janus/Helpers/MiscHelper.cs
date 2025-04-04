﻿using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using System.Text.Json;

namespace Janus.Helpers
{
    public class MiscHelper
    {
        public static bool ConfirmAction(ILogger logger, string message, bool force = false)
        {
            if (force)
            {
                return true;
            }

            while (true)
            {
                logger.Log($"{message} (Y/N)");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Y)
                {
                    logger.Log("");
                    return true;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    logger.Log("");
                    return false;
                }
                else
                {
                    logger.Log("");
                    logger.Log("Invalid input. Please confirm 'Y' or 'N'");
                }
            }
        }





        public static string GetUsername()
        {
            var credManager = new CredentialManager();
            var credentials = credManager.LoadCredentials();

            // Use credentials if valid
            if (credentials != null && !string.IsNullOrEmpty(credentials.Username))
            {
                return credentials.Username;
            }

            // Fallback to system username
            string envUsername = Environment.UserName;

            return string.IsNullOrEmpty(envUsername) ? "" : envUsername;

        }

        public static string GetEmail()
        {
            var credManager = new CredentialManager();
            var credentials = credManager.LoadCredentials();

            // Return email if valid
            if (credentials != null && !string.IsNullOrEmpty(credentials.Email))
            {
                return credentials.Email;
            }

            // No email configured
            return "";
        }







        public static (string, string) CreateInitData()
        {
            string initialCommitMessage = "Initial commit";
            string emptyTreeHash = "";
            string branchName = "main";
            string? parentHash = null;
            string? authorName = "janus";
            string? authorEmail = "janus";

            string initCommitHash = HashHelper.ComputeCommitHash(parentHash, branchName, authorName, authorEmail, DateTime.UtcNow, initialCommitMessage, emptyTreeHash);

            string commitMetadata = GenerateCommitMetadata(branchName, initCommitHash, emptyTreeHash, initialCommitMessage, new List<string> { }, authorName, authorEmail);

            return (initCommitHash, commitMetadata);
        }







        public static bool ValidateRepoExists(ILogger Logger, Paths paths)
        {
            if (!Directory.Exists(paths.JanusDir))
            {
                Logger.Log("Not a janus repository. Use 'init' command to initialise a repository");

                return false;
            }

            return true;
        }



        public static string GetCurrentBranchRelPath(Paths paths)
        {
            // Ensure the HEAD file exists
            if (!File.Exists(paths.HEAD))
            {
                throw new FileNotFoundException("HEAD file not found. The repository may not be initialised correctly", paths.HEAD);
            }

            // Check contents of HEAD
            var pointer = File.ReadAllText(paths.HEAD).Trim();

            if (string.IsNullOrWhiteSpace(pointer))
            {
                throw new InvalidDataException("HEAD file is empty or invalid");
            }

            if (!pointer.StartsWith("ref: "))
            {
                throw new InvalidDataException("HEAD file does not point to a valid reference");
            }

            // Get the ref path from the pointer
            var refPath = pointer.Substring(5); // Remove "ref: "

            return refPath;
        }

        public static string GetCurrentBranchName(Paths paths)
        {
            // Get path to the current branch
            string path = GetCurrentBranchRelPath(paths);

            //...branches/branchName/head
            string branchDir = Path.GetDirectoryName(path);

            // Get the current branch name
            return Path.GetFileName(branchDir);
        }




        public static string GetCurrentHeadCommitHash(Paths paths)
        {
            // Ensure the HEAD file exists
            if (!File.Exists(paths.HEAD))
            {
                throw new FileNotFoundException("HEAD file not found. The repository may not be initialised correctly", paths.HEAD);
            }

            // Check contents of HEAD
            string refPath = GetCurrentBranchRelPath(paths);

            var fullRefPath = Path.Combine(paths.JanusDir, refPath);

            // Ensure the ref file exists
            if (!File.Exists(fullRefPath))
            {
                throw new FileNotFoundException($"Reference file not found for {refPath}", fullRefPath);
            }

            // Read the commit hash from the ref file
            var commitHash = File.ReadAllText(fullRefPath).Trim();

            if (commitHash == null)
            {
                throw new InvalidDataException("Reference file contains an invalid commit hash");
            }

            return commitHash;
        }





        public static string GenerateCommitMetadata(string branch, string commitHash, string treeHash, string commitMessage, List<string> parentCommits, string authorName, string authorEmail)
        {
            var metadata = new CommitMetadata
            {
                Commit = commitHash,
                Parents = parentCommits,
                Branch = branch,
                AuthorName = authorName,
                AuthorEmail = authorEmail,
                Date = DateTime.UtcNow,
                Message = commitMessage,
                Tree = treeHash
            };

            string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });

            return metadataJson;
        }



        public static void DisplaySeperator(ILogger logger)
        {
            logger.Log(new string('-', 50));
        }





    }
}
