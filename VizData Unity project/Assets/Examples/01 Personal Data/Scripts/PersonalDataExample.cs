using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalDataExample : MonoBehaviour
{
    public string dataCsvFileName = "";

    List<Person> _people = new List<Person>();

    int _ageMin, _ageMax;

    void Awake()
    {
        //Parse
        string csvFilePath = Application.streamingAssetsPath + "/" + dataCsvFileName;
        string csvContent = File.ReadAllText(csvFilePath);
        Parse( csvContent );

        //Filter
        Filter();

        //Mine
        Mine();

        //Represent
        Represent();

    }

 
    void Parse( string csvText)
    {
        string[] rowContents = csvText.Split('\n');

        for( int r = 0; r < rowContents.Length; r++)
        {
            string rowContent = rowContents[r];
            string[] fieldContents = rowContent.Split(',');
            Person person = new Person( r );

            //for each field in this row
            for (int q = 0; q < fieldContents.Length; q++)
            {
                string fieldContent = fieldContents[q];

                switch( q )

                {
                    case 0:
                        // First Name
                        person.firstName = fieldContent;
                        break;

                    case 1:
                        // Last Name
                        person.lastName = fieldContent;
                        break;

                    case 2:
                        // Age
                        int age;
                        bool parseSucceeded = int.TryParse(fieldContent, out age);
                        if( parseSucceeded) person.age = age;
                        break;

                    case 3:
                        // Had Covid
                        person.hadCovid = fieldContent.ToLower() == "yes";
                        break;

                    case 6:
                        //Post Number
                        int postNumber;
                        parseSucceeded = int.TryParse(fieldContent, out postNumber);
                        if (parseSucceeded) person.postNumber = postNumber;
                        break;

                    case 7:
                        //Had Pet
                        person.hadPet = fieldContent.ToLower() == "yes";
                        break;

                    case 8:
                        //Number of Cohabitants
                        int numberOfCohabitantsCount;
                        parseSucceeded = int.TryParse(fieldContent, out numberOfCohabitantsCount);
                        if (parseSucceeded) person.numberOfCohabitantsCount = numberOfCohabitantsCount;
                        break;

                    case 9:
                        //Steam Games
                        int numberOfSteamGamesCount;
                        parseSucceeded = int.TryParse(fieldContent, out numberOfSteamGamesCount);
                        if (parseSucceeded) person.numberOfSteamGamesCount = numberOfSteamGamesCount;
                        break;

                    case 10:
                        //Sibling count
                        int siblingCount;
                        parseSucceeded = int.TryParse(fieldContent, out siblingCount);
                        if (parseSucceeded) person.siblingCount = siblingCount;
                        break;


                }

            }

            // Parse covid relation level.
            Person.CovidRelationLevel covidRelationLevel = Person.CovidRelationLevel.None;
            if (fieldContents.Length > 6)
            {
                bool familyHadCovid, familyOrFriendsHadCovid, anyoneHadCovid;
                if (
                    bool.TryParse(fieldContents[4], out familyHadCovid) &&
                    bool.TryParse(fieldContents[5], out familyOrFriendsHadCovid) &&
                    bool.TryParse(fieldContents[6], out anyoneHadCovid)
                )
                {
                    if (anyoneHadCovid) covidRelationLevel = Person.CovidRelationLevel.Anyone;
                    else if (familyOrFriendsHadCovid) covidRelationLevel = Person.CovidRelationLevel.FamilyorFriend;
                    else if (familyOrFriendsHadCovid) covidRelationLevel = Person.CovidRelationLevel.Family;
                }
            }

            _people.Add(person);
        }
    }

    void Filter()
    {
        for(int p = _people.Count-1; p >= 0; p-- ){
            Person person = _people[p];

            if(person.age < 18|| person.age > 127) { //if too young or too old
                    _people.RemoveAt(p);

            }
        }

    }

    void Mine()
    {
        _ageMin = int.MaxValue;
        _ageMax = int.MinValue;
        foreach(Person person in _people)
        {
            if (person.age > _ageMax) _ageMax = person.age;
            else if (person.age < _ageMin) _ageMin = person.age;
        }

    }
    

    void Represent()
    {
        for(int p = 0; p <_people.Count; p++)
        {
            Person person = _people[p];

            float x = p;
            float height = Mathf.InverseLerp(_ageMin, _ageMax, person.age) * 10;
            float y = height *0.5f;
            float width = 0.5f;

            GameObject mainObject = new GameObject(person.id + " " + person.firstName);
            mainObject.transform.SetParent(transform);
            mainObject.transform.localPosition = new Vector3(x,y,0);

            GameObject barObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barObject.transform.SetParent(mainObject.transform);
            barObject.transform.localPosition = Vector3.zero;
            barObject.transform.localScale = new Vector3(width, height, width);

        }
    }
}
