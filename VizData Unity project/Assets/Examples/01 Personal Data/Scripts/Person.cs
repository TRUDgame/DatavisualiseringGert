
public class Person 
{

    public int id;
    public string firstName;
    public string lastName;
    public int age;
    public bool hadCovid;
    public CovidRelationLevel covidRelationLevel;
    public int postNumber;
    public bool hadPet;
    public int numberOfCohabitantsCount;
    public int numberOfSteamGamesCount;
    public int siblingCount;


    public enum CovidRelationLevel

    {
        Family, FamilyorFriend, Anyone, None
    }


    public Person (int id)

    {
        this.id = id;
    }


}
