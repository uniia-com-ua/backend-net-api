namespace UniiaAdmin.WebApi.Constants
{
    public static class ErrorMessages
    {
        public const string IdRequired = "The ID parameter is required";

        public const string ModelNotValid = "Model is not valid";

        public const string OnlyJpgAllowed = "Only JPG/JPEG files are allowed";

        public const string OnlyPdfAllowed = "Only PDF files are allowed";

        public static string ModelNotFound(string modelName, string id) => $"{modelName} with ID '{id}' not found";

        public static string FileParsingFailed(string? fileId) => $"Parsing file with ID '{fileId}' failed";

        public static string ModelFileWithIdNotFound(string modelName, string id) => $"File for {modelName.ToLower()} with ID '{id}' not found.";

        public static string ModelFileIdIsNull(string modelName) => $"File for {modelName.ToLower()} is empty or null.";

        public static string ModelFileMissing(string modelName, string authorId) =>
            $"{modelName} file with ID '{authorId}' not found. Please, delete the {modelName.ToLower()} model and create him again";
    }
}
