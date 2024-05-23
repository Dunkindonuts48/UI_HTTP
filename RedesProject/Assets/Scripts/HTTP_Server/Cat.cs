using System;
using UnityEngine;

namespace HTTP_NET_Project
{
    [Serializable]
    class Cat
    {
        //[SerializeField]
            private string name; // Key to search
      //  [SerializeField]
        private string breed;
    //    [SerializeField]
        private string gender;
  //      [SerializeField]
        private int age;
//        [SerializeField]
        private string owner;

        public Cat(string name, string breed, string gender, int age, string owner)
        {
            Name = name;
            Breed = breed;
            Gender = gender;
            Age = age;
            Owner = owner;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Breed
        {
            get { return breed; }
            set { breed = value; }
        }

        public string Gender
        {
            get { return gender; }
            set { gender = value; }
        }

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        public string Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public override string ToString()
        {
            return $"Name: {Name}\n" +
                $"Breed: {Breed}\n" +
                $"Gender: {Gender}\n" +
                $"Age: {Age}\n" +
                $"Owner: {Owner}";
        }

        public string ToJson()
        {
            // Implement here
            return JsonUtility.ToJson(this); 
        }
    }
}