namespace DirectoryService.Domain;

public static class Constants
{
    public const string SoftDeletedLabel = "deleted-";
    public static class DepartmentConstants
    {

        public const int NameMinLength = 3;
        public const int NameMaxLength = 150;

        public const int IdentifierMinLength = 3;
        public const int IdentifierMaxLength = 150;

    }    
    
    public static class PositionConstants
    {

        public const int NameMinLength = 3;
        public const int NameMaxLength = 100;
        
        public const int DescriptionMaxLength = 1000;
    }

    public static class LocationConstants
    {

        public const int NameMinLength = 3;
        public const int NameMaxLength = 120;
    }
}