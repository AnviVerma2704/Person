﻿using System;
using System.Collections.Generic;
using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
 public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
  public ApplicationDbContext(DbContextOptions options) : base(options)
  {
  }

  public virtual DbSet<Country> Countries { get; set; }
  public virtual DbSet<Person> Persons { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
   base.OnModelCreating(modelBuilder);

   modelBuilder.Entity<Country>().ToTable("Countries");
   modelBuilder.Entity<Person>().ToTable("Persons");

   //Seed to Countries
   string countriesJson = System.IO.File.ReadAllText("countries.json");
   List<Country> countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson);

   foreach (Country country in countries)
    modelBuilder.Entity<Country>().HasData(country);


   //Seed to Persons
   string personsJson = System.IO.File.ReadAllText("persons.json");
   List<Person> persons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personsJson);

   foreach (Person person in persons)
    modelBuilder.Entity<Person>().HasData(person);

   //modelBuilder.Entity<Person>()
   //  .HasIndex(temp => temp.TIN).IsUnique();

   //Table Relations
   modelBuilder.Entity<Person>(entity =>
   {
    entity.HasOne<Country>(c => c.Country)
       .WithMany(p => p.Persons)
       .HasForeignKey(p => p.CountryID);
   });
  }

  public List<Person> sp_GetAllPersons()
  {
   return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
  }

  public int sp_InsertPerson(Person person)
  {
   SqlParameter[] parameters = new SqlParameter[] {
        new SqlParameter("@PersonID", person.PersonID),
        new SqlParameter("@PersonName", person.PersonName),
        new SqlParameter("@Email", person.Email),
        new SqlParameter("@DateOfBirth", person.DateOfBirth),
        new SqlParameter("@Gender", person.Gender),
        new SqlParameter("@CountryID", person.CountryID),
        new SqlParameter("@Address", person.Address),
        new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters)
      };

   return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", parameters);
  }
 }
}
