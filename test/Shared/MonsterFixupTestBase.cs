// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.MonsterModel;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class MonsterFixupTestBase
    {
        private const string SnapshotDatabaseName = "MonsterSnapshot";
        private const string FullNotifyDatabaseName = "MonsterFullNotify";
        private const string ChangedOnlyDatabaseName = "MonsterChangedOnly";

        [Fact]
        public virtual async Task Can_build_monster_model_and_seed_data_using_FKs()
        {
            await Can_build_monster_model_and_seed_data_using_FKs_test(CreateSnapshotMonsterContext, SnapshotDatabaseName);
        }

        [Fact]
        public virtual async Task Can_build_monster_model_with_full_notification_entities_and_seed_data_using_FKs()
        {
            await Can_build_monster_model_and_seed_data_using_FKs_test(CreateChangedChangingMonsterContext, FullNotifyDatabaseName);
        }

        [Fact]
        public virtual async Task Can_build_monster_model_with_changed_only_notification_entities_and_seed_data_using_FKs()
        {
            await Can_build_monster_model_and_seed_data_using_FKs_test(CreateChangedOnlyMonsterContext, ChangedOnlyDatabaseName);
        }

        private async Task Can_build_monster_model_and_seed_data_using_FKs_test(
            Func<IServiceProvider, MonsterContext> createContext, string databaseName)
        {
            var serviceProvider = CreateServiceProvider();
            
            await CreateAndSeedDatabase(databaseName, () => createContext(serviceProvider));

            SimpleVerification(() => createContext(serviceProvider));
            FkVerification(() => createContext(serviceProvider));
            NavigationVerification(() => createContext(serviceProvider));
        }

        [Fact]
        public virtual async Task One_to_many_fixup_happens_when_FKs_change_for_snapshot_entities()
        {
            await One_to_many_fixup_happens_when_FKs_change_test(CreateSnapshotMonsterContext, SnapshotDatabaseName, useDetectChanges: true);
        }

        [Fact]
        public virtual async Task One_to_many_fixup_happens_when_FKs_change_for_full_notification_entities()
        {
            await One_to_many_fixup_happens_when_FKs_change_test(CreateChangedChangingMonsterContext, FullNotifyDatabaseName, useDetectChanges: false);
        }

        [Fact]
        public virtual async Task One_to_many_fixup_happens_when_FKs_change_for_changed_only_notification_entities()
        {
            await One_to_many_fixup_happens_when_FKs_change_test(CreateChangedOnlyMonsterContext, ChangedOnlyDatabaseName, useDetectChanges: false);
        }

        private async Task One_to_many_fixup_happens_when_FKs_change_test(
            Func<IServiceProvider, MonsterContext> createContext, string databaseName, bool useDetectChanges)
        {
            var serviceProvider = CreateServiceProvider();

            await CreateAndSeedDatabase(databaseName, () => createContext(serviceProvider));

            using (var context = createContext(serviceProvider))
            {
                var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
                var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
                var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

                var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
                var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
                var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

                Assert.Same(login2, message2.Sender);
                Assert.Same(message2, login2.SentMessages.Single());

                Assert.Same(login2, message1.Recipient);
                Assert.Same(login2, message3.Recipient);
                Assert.Equal(new[] { message1, message3 }, login2.ReceivedMessages.OrderBy(m => m.Body).ToArray());

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                Assert.Empty(login3.SentMessages);

                // Simple change
                message2.FromUsername = login3.Username;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Same(login3, message2.Sender);
                Assert.Same(message2, login3.SentMessages.Single());

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                Assert.Empty(login2.SentMessages);

                // Change back
                message2.FromUsername = login2.Username;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Same(login2, message2.Sender);
                Assert.Same(message2, login2.SentMessages.Single());

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                Assert.Empty(login3.SentMessages);

                // Remove the relationship
                message2.FromUsername = null;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(message2.Sender);
                Assert.Empty(login2.SentMessages);

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                Assert.Empty(login3.SentMessages);

                // Put the relationship back
                message2.FromUsername = login3.Username;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Same(login3, message2.Sender);
                Assert.Same(message2, login3.SentMessages.Single());

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                Assert.Empty(login2.SentMessages);
            }
        }

        [Fact]
        public virtual async Task One_to_many_fixup_happens_when_reference_changes_for_snapshot_entities()
        {
            await One_to_many_fixup_happens_when_reference_changes_test(CreateSnapshotMonsterContext, SnapshotDatabaseName, useDetectChanges: true);
        }

        [Fact]
        public virtual async Task One_to_many_fixup_happens_when_reference_changes_for_full_notification_entities()
        {
            await One_to_many_fixup_happens_when_reference_changes_test(CreateChangedChangingMonsterContext, FullNotifyDatabaseName, useDetectChanges: false);
        }

        [Fact]
        public virtual async Task One_to_many_fixup_happens_when_reference_changes_for_changed_only_notification_entities()
        {
            await One_to_many_fixup_happens_when_reference_changes_test(CreateChangedOnlyMonsterContext, ChangedOnlyDatabaseName, useDetectChanges: false);
        }

        private async Task One_to_many_fixup_happens_when_reference_changes_test(
            Func<IServiceProvider, MonsterContext> createContext, string databaseName, bool useDetectChanges)
        {
            var serviceProvider = CreateServiceProvider();

            await CreateAndSeedDatabase(databaseName, () => createContext(serviceProvider));

            using (var context = createContext(serviceProvider))
            {
                var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
                var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
                var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

                var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
                var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
                var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

                Assert.Same(login2, message2.Sender);
                Assert.Same(message2, login2.SentMessages.Single());

                Assert.Same(login2, message1.Recipient);
                Assert.Same(login2, message3.Recipient);
                Assert.Equal(new[] { message1, message3 }, login2.ReceivedMessages.OrderBy(m => m.Body).ToArray());

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                Assert.Empty(login3.SentMessages);

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);

                // Simple change
                message2.Sender = login3;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                // TODO: Just testing FK fixup for now; inverse nav fixup comes later

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login3.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);

                // Change back
                message2.Sender = login2;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);

                // Remove the relationship
                message2.Sender = null;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Null(message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);

                // Put the relationship back
                message2.Sender = login3;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login3.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);
            }
        }

        [Fact]
        public virtual async Task One_to_many_fixup_happens_when_collection_changes_for_snapshot_entities()
        {
            await One_to_many_fixup_happens_when_collection_changes_test(CreateSnapshotMonsterContext, SnapshotDatabaseName, useDetectChanges: true);
        }

        //[Fact] TODO: Support INotifyCollectionChanging so that collection change detection without DetectChanges works
        public virtual async Task One_to_many_fixup_happens_when_collection_changes_for_full_notification_entities()
        {
            await One_to_many_fixup_happens_when_collection_changes_test(CreateChangedChangingMonsterContext, FullNotifyDatabaseName, useDetectChanges: false);
        }

        //[Fact] TODO: Support INotifyCollectionChanging so that collection change detection without DetectChanges works
        public virtual async Task One_to_many_fixup_happens_when_collection_changes_for_changed_only_notification_entities()
        {
            await One_to_many_fixup_happens_when_collection_changes_test(CreateChangedOnlyMonsterContext, ChangedOnlyDatabaseName, useDetectChanges: false);
        }

        private async Task One_to_many_fixup_happens_when_collection_changes_test(
            Func<IServiceProvider, MonsterContext> createContext, string databaseName, bool useDetectChanges)
        {
            var serviceProvider = CreateServiceProvider();

            await CreateAndSeedDatabase(databaseName, () => createContext(serviceProvider));

            using (var context = createContext(serviceProvider))
            {
                var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
                var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
                var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

                var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
                var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
                var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

                Assert.Same(login2, message2.Sender);
                Assert.Same(message2, login2.SentMessages.Single());

                Assert.Same(login2, message1.Recipient);
                Assert.Same(login2, message3.Recipient);
                Assert.Equal(new[] { message1, message3 }, login2.ReceivedMessages.OrderBy(m => m.Body).ToArray());

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                Assert.Empty(login3.SentMessages);

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);

                // Remove entities
                login2.ReceivedMessages.Remove(message3);
                login1.ReceivedMessages.Remove(message2);

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                // TODO: Just testing FK fixup for now; inverse nav fixup comes later

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Null(message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Null(message3.ToUsername);

                // Add entities
                login1.ReceivedMessages.Add(message3);
                login2.ReceivedMessages.Add(message2);

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Equal(login2.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login1.Username, message3.ToUsername);

                // Remove and add at the same time
                login2.ReceivedMessages.Remove(message2);
                login1.ReceivedMessages.Remove(message3);
                login1.ReceivedMessages.Add(message2);
                login2.ReceivedMessages.Add(message3);

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);
            }
        }

        [Fact]
        public virtual async Task One_to_one_fixup_happens_when_FKs_change_for_snapshot_entities()
        {
            await One_to_one_fixup_happens_when_FKs_change_test(CreateSnapshotMonsterContext, SnapshotDatabaseName, useDetectChanges: true);
        }

        [Fact]
        public virtual async Task One_to_one_fixup_happens_when_FKs_change_for_full_notification_entities()
        {
            await One_to_one_fixup_happens_when_FKs_change_test(CreateChangedChangingMonsterContext, FullNotifyDatabaseName, useDetectChanges: false);
        }

        [Fact]
        public virtual async Task One_to_one_fixup_happens_when_FKs_change_for_changed_only_notification_entities()
        {
            await One_to_one_fixup_happens_when_FKs_change_test(CreateChangedOnlyMonsterContext, ChangedOnlyDatabaseName, useDetectChanges: false);
        }

        private async Task One_to_one_fixup_happens_when_FKs_change_test(
            Func<IServiceProvider, MonsterContext> createContext, string databaseName, bool useDetectChanges)
        {
            var serviceProvider = CreateServiceProvider();

            await CreateAndSeedDatabase(databaseName, () => createContext(serviceProvider));

            using (var context = createContext(serviceProvider))
            {
                var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
                var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
                var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
                var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

                Assert.Null(customer0.Husband);
                Assert.Same(customer2, customer0.Wife);

                Assert.Null(customer1.Husband);
                Assert.Null(customer1.Wife);

                Assert.Same(customer0, customer2.Husband);
                Assert.Null(customer2.Wife);

                Assert.Null(customer3.Husband);
                Assert.Null(customer3.Wife);

                // Add a new relationship
                customer1.HusbandId = customer3.CustomerId;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(customer0.Husband);
                Assert.Same(customer2, customer0.Wife);

                Assert.Same(customer3, customer1.Husband);
                Assert.Null(customer1.Wife);

                Assert.Same(customer0, customer2.Husband);
                Assert.Null(customer2.Wife);

                Assert.Null(customer3.Husband);
                Assert.Same(customer1, customer3.Wife);

                // Remove the relationship
                customer1.HusbandId = null;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(customer0.Husband);
                Assert.Same(customer2, customer0.Wife);

                Assert.Null(customer1.Husband);
                Assert.Null(customer1.Wife);

                Assert.Same(customer0, customer2.Husband);
                Assert.Null(customer2.Wife);

                Assert.Null(customer3.Husband);
                Assert.Null(customer3.Wife);

                // Change existing relationship
                customer2.HusbandId = customer3.CustomerId;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(customer0.Husband);
                Assert.Null(customer0.Wife);

                Assert.Null(customer1.Husband);
                Assert.Null(customer1.Wife);

                Assert.Same(customer3, customer2.Husband);
                Assert.Null(customer2.Wife);

                Assert.Null(customer3.Husband);
                Assert.Same(customer2, customer3.Wife);

                // Give Tarquin a husband and a wife
                customer3.HusbandId = customer2.CustomerId;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(customer0.Husband);
                Assert.Null(customer0.Wife);

                Assert.Null(customer1.Husband);
                Assert.Null(customer1.Wife);

                Assert.Same(customer3, customer2.Husband);
                Assert.Same(customer3, customer2.Wife);

                Assert.Same(customer2, customer3.Husband);
                Assert.Same(customer2, customer3.Wife);
            }
        }

        [Fact]
        public virtual async Task One_to_one_fixup_happens_when_reference_change_for_snapshot_entities()
        {
            await One_to_one_fixup_happens_when_reference_change_test(CreateSnapshotMonsterContext, SnapshotDatabaseName, useDetectChanges: true);
        }

        [Fact]
        public virtual async Task One_to_one_fixup_happens_when_reference_change_for_full_notification_entities()
        {
            await One_to_one_fixup_happens_when_reference_change_test(CreateChangedChangingMonsterContext, FullNotifyDatabaseName, useDetectChanges: false);
        }

        [Fact]
        public virtual async Task One_to_one_fixup_happens_when_reference_change_for_changed_only_notification_entities()
        {
            await One_to_one_fixup_happens_when_reference_change_test(CreateChangedOnlyMonsterContext, ChangedOnlyDatabaseName, useDetectChanges: false);
        }

        private async Task One_to_one_fixup_happens_when_reference_change_test(
            Func<IServiceProvider, MonsterContext> createContext, string databaseName, bool useDetectChanges)
        {
            var serviceProvider = CreateServiceProvider();

            await CreateAndSeedDatabase(databaseName, () => createContext(serviceProvider));

            using (var context = createContext(serviceProvider))
            {
                var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
                var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
                var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
                var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

                Assert.Null(customer0.Husband);
                Assert.Same(customer2, customer0.Wife);

                Assert.Null(customer1.Husband);
                Assert.Null(customer1.Wife);

                Assert.Same(customer0, customer2.Husband);
                Assert.Null(customer2.Wife);

                Assert.Null(customer3.Husband);
                Assert.Null(customer3.Wife);

                Assert.Null(customer0.HusbandId);
                Assert.Null(customer1.HusbandId);
                Assert.Equal(customer0.CustomerId, customer2.HusbandId);
                Assert.Null(customer3.HusbandId);

                // Set a dependent
                customer1.Husband = customer3;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                // TODO: Just testing FK fixup for now; inverse nav fixup comes later

                Assert.Null(customer0.HusbandId);
                Assert.Equal(customer3.CustomerId, customer1.HusbandId);
                Assert.Equal(customer0.CustomerId, customer2.HusbandId);
                Assert.Null(customer3.HusbandId);

                // Remove a dependent
                customer2.Husband = null;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(customer0.HusbandId);
                Assert.Equal(customer3.CustomerId, customer1.HusbandId);
                Assert.Null(customer2.HusbandId);
                Assert.Null(customer3.HusbandId);

                // Set a principal
                customer0.Wife = customer3;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }


                Assert.Null(customer0.HusbandId);
                Assert.Equal(customer3.CustomerId, customer1.HusbandId);
                Assert.Null(customer2.HusbandId);
                Assert.Equal(customer0.CustomerId, customer3.HusbandId);

                // Remove a principal
                customer0.Wife = null;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(customer0.HusbandId);
                Assert.Equal(customer3.CustomerId, customer1.HusbandId);
                Assert.Null(customer2.HusbandId);
                Assert.Null(customer3.HusbandId);
            }
        }

        [Fact]
        public virtual async Task Composite_fixup_happens_when_FKs_change_for_snapshot_entities()
        {
            await Composite_fixup_happens_when_FKs_change_test(CreateSnapshotMonsterContext, SnapshotDatabaseName, useDetectChanges: true);
        }

        [Fact]
        public virtual async Task Composite_fixup_happens_when_FKs_change_for_full_notification_entities()
        {
            await Composite_fixup_happens_when_FKs_change_test(CreateChangedChangingMonsterContext, FullNotifyDatabaseName, useDetectChanges: false);
        }

        [Fact]
        public virtual async Task Composite_fixup_happens_when_FKs_change_for_changed_only_notification_entities()
        {
            await Composite_fixup_happens_when_FKs_change_test(CreateChangedOnlyMonsterContext, ChangedOnlyDatabaseName, useDetectChanges: false);
        }

        private async Task Composite_fixup_happens_when_FKs_change_test(
            Func<IServiceProvider, MonsterContext> createContext, string databaseName, bool useDetectChanges)
        {
            var serviceProvider = CreateServiceProvider();

            await CreateAndSeedDatabase(databaseName, () => createContext(serviceProvider));

            using (var context = createContext(serviceProvider))
            {
                var product1 = context.Products.Single(e => e.Description.StartsWith("Mrs"));
                var product2 = context.Products.Single(e => e.Description.StartsWith("Chocolate"));
                var product3 = context.Products.Single(e => e.Description.StartsWith("Assorted"));

                var productReview1 = context.ProductReviews.Single(e => e.Review.StartsWith("Better"));
                var productReview2 = context.ProductReviews.Single(e => e.Review.StartsWith("Good"));
                var productReview3 = context.ProductReviews.Single(e => e.Review.StartsWith("Eeky"));

                var productPhoto1 = context.ProductPhotos.Single(e => e.Photo[0] == 101);
                var productPhoto2 = context.ProductPhotos.Single(e => e.Photo[0] == 103);
                var productPhoto3 = context.ProductPhotos.Single(e => e.Photo[0] == 105);

                var productWebFeature1 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("Waffle"));
                var productWebFeature2 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("What"));

                Assert.Same(productPhoto1, productWebFeature1.Photo);
                Assert.Same(productWebFeature1, productPhoto1.Features.Single());

                Assert.Same(productReview1, productWebFeature1.Review);
                Assert.Same(productWebFeature1, productReview1.Features.Single());

                Assert.Null(productWebFeature2.Photo);
                Assert.Empty(productPhoto2.Features);

                Assert.Same(productReview3, productWebFeature2.Review);
                Assert.Same(productWebFeature2, productReview3.Features.Single());

                Assert.Empty(productPhoto3.Features);
                Assert.Empty(productReview2.Features);

                // Change one part of the key
                productWebFeature1.ProductId = product2.ProductId;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(productWebFeature1.Photo);
                Assert.Empty(productPhoto1.Features);

                Assert.Null(productWebFeature1.Review);
                Assert.Empty(productReview1.Features);

                Assert.Null(productWebFeature2.Photo);
                Assert.Empty(productPhoto2.Features);

                Assert.Same(productReview3, productWebFeature2.Review);
                Assert.Same(productWebFeature2, productReview3.Features.Single());

                Assert.Empty(productPhoto3.Features);
                Assert.Empty(productReview2.Features);

                // Change the other part of the key
                productWebFeature1.ReviewId = productReview3.ReviewId;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Null(productWebFeature1.Photo);
                Assert.Empty(productPhoto1.Features);

                Assert.Same(productReview3, productWebFeature1.Review);
                Assert.Equal(
                    new[] { productWebFeature1, productWebFeature2 },
                    productReview3.Features.OrderBy(f => f.Heading).ToArray());

                Assert.Null(productWebFeature2.Photo);
                Assert.Empty(productPhoto2.Features);

                Assert.Same(productReview3, productWebFeature2.Review);
                Assert.Equal(
                    new[] { productWebFeature1, productWebFeature2 },
                    productReview3.Features.OrderBy(f => f.Heading).ToArray());

                Assert.Empty(productPhoto3.Features);
                Assert.Empty(productReview2.Features);

                // Change both at the same time
                productWebFeature1.ReviewId = productReview1.ReviewId;
                productWebFeature1.ProductId = product1.ProductId;

                if (useDetectChanges)
                {
                    context.ChangeTracker.StateManager.DetectChanges();
                }

                Assert.Same(productPhoto1, productWebFeature1.Photo);
                Assert.Same(productWebFeature1, productPhoto1.Features.Single());

                Assert.Same(productReview1, productWebFeature1.Review);
                Assert.Same(productWebFeature1, productReview1.Features.Single());

                Assert.Null(productWebFeature2.Photo);
                Assert.Empty(productPhoto2.Features);

                Assert.Same(productReview3, productWebFeature2.Review);
                Assert.Same(productWebFeature2, productReview3.Features.Single());

                Assert.Empty(productPhoto3.Features);
                Assert.Empty(productReview2.Features);
            }
        }

        protected void SimpleVerification(Func<MonsterContext> createContext)
        {
            using (var context = createContext())
            {
                Assert.Equal(
                    new[] { "Eeky Bear", "Sheila Koalie", "Sue Pandy", "Tarquin Tiger" },
                    context.Customers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Assorted Dog Treats", "Chocolate Donuts", "Mrs Koalie's Famous Waffles" },
                    context.Products.Select(c => c.Description).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Barcode 1 2 3 4", "Barcode 2 2 3 4", "Barcode 3 2 3 4" },
                    context.Barcodes.Select(c => c.Text).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Barcode 1 2 3 4", "Barcode 2 2 3 4", "Barcode 3 2 3 4" },
                    context.Barcodes.Select(c => c.Text).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Eeky Bear", "Trent" },
                    context.BarcodeDetails.Select(c => c.RegisteredTo).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Treats not Donuts", "Wot no waffles?" },
                    context.IncorrectScans.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Don't give coffee to Eeky!", "Really! Don't give coffee to Eeky!" },
                    context.Complaints.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Destroyed all coffee in Redmond area." },
                    context.Resolutions.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73", "TheStripedMenace" },
                    context.Logins.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Crumbs in the cupboard", "Donuts gone missing", "Pig prints on keyboard" },
                    context.SuspiciousActivities.Select(c => c.Activity).OrderBy(n => n));

                Assert.Equal(
                    new[] { "1234", "2234" },
                    context.RsaTokens.Select(c => c.Serial).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73" },
                    context.SmartCards.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Rent-A-Mole" },
                    context.PasswordResets.Select(c => c.TempPassword).OrderBy(n => n));

                Assert.Equal(
                    new[] { "somePage1", "somePage2", "somePage3" },
                    context.PageViews.Select(c => c.PageUrl).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73" },
                    context.LastLogins.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Fancy a cup of tea?", "I'll put the kettle on.", "Love one!" },
                    context.Messages.Select(c => c.Body).OrderBy(n => n));

                Assert.Equal(
                    new[] { "MrsBossyPants", "MrsKoalie73", "TheStripedMenace" },
                    context.Orders.Select(c => c.Username).OrderBy(n => n));

                Assert.Equal(
                    new[] { "And donuts!", "But no coffee. :-(", "Must have tea!" },
                    context.OrderNotes.Select(c => c.Note).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Eeky Bear", "Eeky Bear", "Eeky Bear" },
                    context.OrderQualityChecks.Select(c => c.CheckedBy).OrderBy(n => n));

                Assert.Equal(
                    new[] { 1, 2, 3, 4, 5, 7 },
                    context.OrderLines.Select(c => c.Quantity).OrderBy(n => n));

                Assert.Equal(
                    new[] { "A Waffle Cart specialty!", "Eeky Bear's favorite!" },
                    context.ProductDetails.Select(c => c.Details).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Better than Tarqies!", "Eeky says yes!", "Good with maple syrup." },
                    context.ProductReviews.Select(c => c.Review).OrderBy(n => n));

                // TODO: Remove ToArray once LINQ to EF7 can support this query
                Assert.Equal(
                    new[] { "101", "103", "105" },
                    context.ProductPhotos.ToArray().Select(c => c.Photo.First().ToString()).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Waffle Style", "What does the waffle say?" },
                    context.ProductWebFeatures.Select(c => c.Heading).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Ants By Boris", "Trading As Trent" },
                    context.Suppliers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "201", "202" },
                    context.SupplierLogos.SelectMany(c => c.Logo).Select(l => l.ToString()).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Orange fur?", "Seems a bit dodgy.", "Very expensive!" },
                    context.SupplierInformation.Select(c => c.Information).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Mrs Bossy Pants!", "Really likes tea." },
                    context.CustomerInformation.Select(c => c.Information).OrderBy(n => n));

                Assert.Equal(
                    new[] { "markash420", "unicorns420" },
                    context.Computers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "It's a Dell!", "It's not a Dell!" },
                    context.ComputerDetails.Select(c => c.Specifications).OrderBy(n => n));

                Assert.Equal(
                    new[] { "Eeky Bear", "Splash Bear" },
                    context.Drivers.Select(c => c.Name).OrderBy(n => n));

                Assert.Equal(
                    new[] { "10", "11" },
                    context.Licenses.Select(c => c.LicenseNumber).OrderBy(n => n));
            }
        }

        protected void FkVerification(Func<MonsterContext> createContext)
        {
            using (var context = createContext())
            {
                var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
                var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
                var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
                var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

                Assert.Null(customer0.HusbandId);
                Assert.Null(customer1.HusbandId);
                Assert.Equal(customer0.CustomerId, customer2.HusbandId);
                Assert.Null(customer3.HusbandId);

                var product1 = context.Products.Single(e => e.Description.StartsWith("Mrs"));
                var product2 = context.Products.Single(e => e.Description.StartsWith("Chocolate"));
                var product3 = context.Products.Single(e => e.Description.StartsWith("Assorted"));

                var barcode1 = context.Barcodes.Single(e => e.Text == "Barcode 1 2 3 4");
                var barcode2 = context.Barcodes.Single(e => e.Text == "Barcode 2 2 3 4");
                var barcode3 = context.Barcodes.Single(e => e.Text == "Barcode 3 2 3 4");

                Assert.Equal(product1.ProductId, barcode1.ProductId);
                Assert.Equal(product2.ProductId, barcode2.ProductId);
                Assert.Equal(product3.ProductId, barcode3.ProductId);

                var barcodeDetails1 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Eeky Bear");
                var barcodeDetails2 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Trent");

                Assert.Equal(barcode1.Code, barcodeDetails1.Code);
                Assert.Equal(barcode2.Code, barcodeDetails2.Code);

                var incorrectScan1 = context.IncorrectScans.Single(e => e.Details.StartsWith("Treats"));
                var incorrectScan2 = context.IncorrectScans.Single(e => e.Details.StartsWith("Wot"));

                Assert.Equal(barcode3.Code, incorrectScan1.ActualCode);
                Assert.Equal(barcode2.Code, incorrectScan1.ExpectedCode);
                Assert.Equal(barcode2.Code, incorrectScan2.ActualCode);
                Assert.Equal(barcode1.Code, incorrectScan2.ExpectedCode);

                var complaint1 = context.Complaints.Single(e => e.Details.StartsWith("Don't"));
                var complaint2 = context.Complaints.Single(e => e.Details.StartsWith("Really"));

                Assert.Equal(customer2.CustomerId, complaint1.CustomerId);
                Assert.Equal(customer2.CustomerId, complaint2.CustomerId);

                var resolution = context.Resolutions.Single(e => e.Details.StartsWith("Destroyed"));

                Assert.Equal(complaint2.ComplaintId, resolution.ResolutionId);

                var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
                var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
                var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

                Assert.Equal(customer1.CustomerId, login1.CustomerId);
                Assert.Equal(customer2.CustomerId, login2.CustomerId);
                Assert.Equal(customer3.CustomerId, login3.CustomerId);

                var suspiciousActivity1 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Pig"));
                var suspiciousActivity2 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Crumbs"));
                var suspiciousActivity3 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Donuts"));

                Assert.Equal(login3.Username, suspiciousActivity1.Username);
                Assert.Equal(login3.Username, suspiciousActivity2.Username);
                Assert.Equal(login3.Username, suspiciousActivity3.Username);

                var rsaToken1 = context.RsaTokens.Single(e => e.Serial == "1234");
                var rsaToken2 = context.RsaTokens.Single(e => e.Serial == "2234");

                Assert.Equal(login1.Username, rsaToken1.Username);
                Assert.Equal(login2.Username, rsaToken2.Username);

                var smartCard1 = context.SmartCards.Single(e => e.Username == "MrsKoalie73");
                var smartCard2 = context.SmartCards.Single(e => e.Username == "MrsBossyPants");

                Assert.Equal(rsaToken1.Serial, smartCard1.CardSerial);
                Assert.Equal(rsaToken2.Serial, smartCard2.CardSerial);
                Assert.Equal(rsaToken1.Issued, smartCard1.Issued);
                Assert.Equal(rsaToken2.Issued, smartCard2.Issued);

                var reset1 = context.PasswordResets.Single(e => e.EmailedTo == "trent@example.com");

                Assert.Equal(login3.Username, reset1.Username);

                var pageView1 = context.PageViews.Single(e => e.PageUrl == "somePage1");
                var pageView2 = context.PageViews.Single(e => e.PageUrl == "somePage1");
                var pageView3 = context.PageViews.Single(e => e.PageUrl == "somePage1");

                Assert.Equal(login1.Username, pageView1.Username);
                Assert.Equal(login1.Username, pageView2.Username);
                Assert.Equal(login1.Username, pageView3.Username);

                var lastLogin1 = context.LastLogins.Single(e => e.Username == "MrsKoalie73");
                var lastLogin2 = context.LastLogins.Single(e => e.Username == "MrsBossyPants");

                Assert.Equal(smartCard1.Username, lastLogin1.SmartcardUsername);
                Assert.Equal(smartCard2.Username, lastLogin2.SmartcardUsername);

                var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
                var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
                var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

                Assert.Equal(login1.Username, message1.FromUsername);
                Assert.Equal(login2.Username, message1.ToUsername);
                Assert.Equal(login2.Username, message2.FromUsername);
                Assert.Equal(login1.Username, message2.ToUsername);
                Assert.Equal(login1.Username, message3.FromUsername);
                Assert.Equal(login2.Username, message3.ToUsername);

                var order1 = context.Orders.Single(e => e.Username == "MrsKoalie73");
                var order2 = context.Orders.Single(e => e.Username == "MrsBossyPants");
                var order3 = context.Orders.Single(e => e.Username == "TheStripedMenace");

                Assert.Equal(customer1.CustomerId, order1.CustomerId);
                Assert.Equal(customer2.CustomerId, order2.CustomerId);
                Assert.Equal(customer3.CustomerId, order3.CustomerId);

                var orderLine1 = context.OrderLines.Single(e => e.Quantity == 7);
                var orderLine2 = context.OrderLines.Single(e => e.Quantity == 1);
                var orderLine3 = context.OrderLines.Single(e => e.Quantity == 2);
                var orderLine4 = context.OrderLines.Single(e => e.Quantity == 3);
                var orderLine5 = context.OrderLines.Single(e => e.Quantity == 4);
                var orderLine6 = context.OrderLines.Single(e => e.Quantity == 5);

                Assert.Equal(product1.ProductId, orderLine1.ProductId);
                Assert.Equal(product2.ProductId, orderLine2.ProductId);
                Assert.Equal(product3.ProductId, orderLine3.ProductId);
                Assert.Equal(product2.ProductId, orderLine4.ProductId);
                Assert.Equal(product1.ProductId, orderLine5.ProductId);
                Assert.Equal(product2.ProductId, orderLine6.ProductId);
                Assert.Equal(order1.AnOrderId, orderLine1.OrderId);
                Assert.Equal(order1.AnOrderId, orderLine2.OrderId);
                Assert.Equal(order2.AnOrderId, orderLine3.OrderId);
                Assert.Equal(order2.AnOrderId, orderLine4.OrderId);
                Assert.Equal(order2.AnOrderId, orderLine5.OrderId);
                Assert.Equal(order3.AnOrderId, orderLine6.OrderId);

                var productDetail1 = context.ProductDetails.Single(e => e.Details.StartsWith("A"));
                var productDetail2 = context.ProductDetails.Single(e => e.Details.StartsWith("Eeky"));

                Assert.Equal(product1.ProductId, productDetail1.ProductId);
                Assert.Equal(product2.ProductId, productDetail2.ProductId);

                var productReview1 = context.ProductReviews.Single(e => e.Review.StartsWith("Better"));
                var productReview2 = context.ProductReviews.Single(e => e.Review.StartsWith("Good"));
                var productReview3 = context.ProductReviews.Single(e => e.Review.StartsWith("Eeky"));

                Assert.Equal(product1.ProductId, productReview1.ProductId);
                Assert.Equal(product1.ProductId, productReview2.ProductId);
                Assert.Equal(product2.ProductId, productReview3.ProductId);

                var productPhoto1 = context.ProductPhotos.Single(e => e.Photo[0] == 101);
                var productPhoto2 = context.ProductPhotos.Single(e => e.Photo[0] == 103);
                var productPhoto3 = context.ProductPhotos.Single(e => e.Photo[0] == 105);

                Assert.Equal(product1.ProductId, productPhoto1.ProductId);
                Assert.Equal(product1.ProductId, productPhoto2.ProductId);
                Assert.Equal(product3.ProductId, productPhoto3.ProductId);

                var productWebFeature1 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("Waffle"));
                var productWebFeature2 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("What"));

                Assert.Equal(product1.ProductId, productWebFeature1.ProductId);
                Assert.Equal(product2.ProductId, productWebFeature2.ProductId);
                Assert.Equal(productPhoto1.PhotoId, productWebFeature1.PhotoId);
                Assert.Null(productWebFeature2.PhotoId);
                Assert.Equal(productReview1.ReviewId, productWebFeature1.ReviewId);
                Assert.Equal(productReview3.ReviewId, productWebFeature2.ReviewId);

                var supplier1 = context.Suppliers.Single(e => e.Name.StartsWith("Trading"));
                var supplier2 = context.Suppliers.Single(e => e.Name.StartsWith("Ants"));

                var supplierLogo1 = context.SupplierLogos.Single(e => e.Logo[0] == 201);

                Assert.Equal(supplier1.SupplierId, supplierLogo1.SupplierId);

                var supplierInfo1 = context.SupplierInformation.Single(e => e.Information.StartsWith("Seems"));
                var supplierInfo2 = context.SupplierInformation.Single(e => e.Information.StartsWith("Orange"));
                var supplierInfo3 = context.SupplierInformation.Single(e => e.Information.StartsWith("Very"));

                Assert.Equal(supplier1.SupplierId, supplierInfo1.SupplierId);
                Assert.Equal(supplier1.SupplierId, supplierInfo2.SupplierId);
                Assert.Equal(supplier2.SupplierId, supplierInfo3.SupplierId);

                var customerInfo1 = context.CustomerInformation.Single(e => e.Information.StartsWith("Really"));
                var customerInfo2 = context.CustomerInformation.Single(e => e.Information.StartsWith("Mrs"));

                Assert.Equal(customer1.CustomerId, customerInfo1.CustomerInfoId);
                Assert.Equal(customer2.CustomerId, customerInfo2.CustomerInfoId);

                var computer1 = context.Computers.Single(e => e.Name == "markash420");
                var computer2 = context.Computers.Single(e => e.Name == "unicorns420");

                var computerDetail1 = context.ComputerDetails.Single(e => e.Specifications == "It's a Dell!");
                var computerDetail2 = context.ComputerDetails.Single(e => e.Specifications == "It's not a Dell!");

                Assert.Equal(computer1.ComputerId, computerDetail1.ComputerDetailId);
                Assert.Equal(computer2.ComputerId, computerDetail2.ComputerDetailId);

                var driver1 = context.Drivers.Single(e => e.Name == "Eeky Bear");
                var driver2 = context.Drivers.Single(e => e.Name == "Splash Bear");

                // TODO: Quering for actual entity currently throws, so projecting to just FK instead
                var licenseName1 = context.Licenses.Where(e => e.LicenseNumber == "10").Select(e => e.Name).Single();
                var licenseName2 = context.Licenses.Where(e => e.LicenseNumber == "11").Select(e => e.Name).Single();

                Assert.Equal(driver1.Name, licenseName1);
                Assert.Equal(driver2.Name, licenseName2);
            }
        }

        protected void NavigationVerification(Func<MonsterContext> createContext)
        {
            using (var context = createContext())
            {
                var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
                var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
                var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
                var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

                Assert.Null(customer0.Husband);
                Assert.Same(customer2, customer0.Wife);

                Assert.Null(customer1.Husband);
                Assert.Null(customer1.Wife);

                Assert.Same(customer0, customer2.Husband);
                Assert.Null(customer2.Wife);

                Assert.Null(customer3.Husband);
                Assert.Null(customer3.Wife);

                var product1 = context.Products.Single(e => e.Description.StartsWith("Mrs"));
                var product2 = context.Products.Single(e => e.Description.StartsWith("Chocolate"));
                var product3 = context.Products.Single(e => e.Description.StartsWith("Assorted"));

                var barcode1 = context.Barcodes.Single(e => e.Text == "Barcode 1 2 3 4");
                var barcode2 = context.Barcodes.Single(e => e.Text == "Barcode 2 2 3 4");
                var barcode3 = context.Barcodes.Single(e => e.Text == "Barcode 3 2 3 4");

                Assert.Same(barcode1, product1.Barcodes.Single());
                Assert.Same(product1, barcode1.Product);

                Assert.Same(barcode2, product2.Barcodes.Single());
                Assert.Same(product2, barcode2.Product);

                Assert.Same(barcode3, product3.Barcodes.Single());
                Assert.Same(product3, barcode3.Product);

                var barcodeDetails1 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Eeky Bear");
                var barcodeDetails2 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Trent");

                Assert.Same(barcodeDetails1, barcode1.Detail);
                Assert.Same(barcodeDetails2, barcode2.Detail);

                var incorrectScan1 = context.IncorrectScans.Single(e => e.Details.StartsWith("Treats"));
                var incorrectScan2 = context.IncorrectScans.Single(e => e.Details.StartsWith("Wot"));

                Assert.Same(barcode3, incorrectScan1.ActualBarcode);
                Assert.Same(barcode2, incorrectScan2.ActualBarcode);

                Assert.Same(barcode2, incorrectScan1.ExpectedBarcode);
                Assert.Same(incorrectScan1, barcode2.BadScans.Single());

                Assert.Same(barcode1, incorrectScan2.ExpectedBarcode);
                Assert.Same(incorrectScan2, barcode1.BadScans.Single());

                Assert.Empty(barcode3.BadScans);

                var complaint1 = context.Complaints.Single(e => e.Details.StartsWith("Don't"));
                var complaint2 = context.Complaints.Single(e => e.Details.StartsWith("Really"));

                Assert.Same(customer2, complaint1.Customer);
                Assert.Same(customer2, complaint2.Customer);

                var resolution = context.Resolutions.Single(e => e.Details.StartsWith("Destroyed"));

                Assert.Same(complaint2, resolution.Complaint);
                Assert.Same(resolution, complaint2.Resolution);

                Assert.Null(complaint1.Resolution);

                var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
                var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
                var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

                Assert.Same(customer1, login1.Customer);
                Assert.Same(login1, customer1.Logins.Single());

                Assert.Same(customer2, login2.Customer);
                Assert.Same(login2, customer2.Logins.Single());

                Assert.Same(customer3, login3.Customer);
                Assert.Same(login3, customer3.Logins.Single());

                Assert.Empty(customer0.Logins);

                var rsaToken1 = context.RsaTokens.Single(e => e.Serial == "1234");
                var rsaToken2 = context.RsaTokens.Single(e => e.Serial == "2234");

                Assert.Same(login1, rsaToken1.Login);
                Assert.Same(login2, rsaToken2.Login);

                var smartCard1 = context.SmartCards.Single(e => e.Username == "MrsKoalie73");
                var smartCard2 = context.SmartCards.Single(e => e.Username == "MrsBossyPants");

                Assert.Same(login1, smartCard1.Login);
                Assert.Same(login2, smartCard2.Login);

                var reset1 = context.PasswordResets.Single(e => e.EmailedTo == "trent@example.com");

                Assert.Same(login3, reset1.Login);

                var pageView1 = context.PageViews.Single(e => e.PageUrl == "somePage1");
                var pageView2 = context.PageViews.Single(e => e.PageUrl == "somePage1");
                var pageView3 = context.PageViews.Single(e => e.PageUrl == "somePage1");

                Assert.Same(login1, pageView1.Login);
                Assert.Same(login1, pageView2.Login);
                Assert.Same(login1, pageView3.Login);

                var lastLogin1 = context.LastLogins.Single(e => e.Username == "MrsKoalie73");
                var lastLogin2 = context.LastLogins.Single(e => e.Username == "MrsBossyPants");

                Assert.Same(login1, lastLogin1.Login);
                Assert.Same(login2, lastLogin2.Login);

                var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
                var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
                var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

                Assert.Same(login1, message1.Sender);
                Assert.Same(login1, message3.Sender);
                Assert.Equal(
                    new[] { "Fanc", "I'll" },
                    login1.SentMessages.Select(m => m.Body.Substring(0, 4)).OrderBy(m => m).ToArray());

                Assert.Same(login2, message2.Sender);
                Assert.Same(message2, login2.SentMessages.Single());

                Assert.Same(login2, message1.Recipient);
                Assert.Same(login2, message3.Recipient);
                Assert.Equal(
                    new[] { "Fanc", "I'll" },
                    login2.ReceivedMessages.Select(m => m.Body.Substring(0, 4)).OrderBy(m => m).ToArray());

                Assert.Same(login1, message2.Recipient);
                Assert.Same(message2, login1.ReceivedMessages.Single());

                var order1 = context.Orders.Single(e => e.Username == "MrsKoalie73");
                var order2 = context.Orders.Single(e => e.Username == "MrsBossyPants");
                var order3 = context.Orders.Single(e => e.Username == "TheStripedMenace");

                Assert.Same(customer1, order1.Customer);
                Assert.Same(order1, customer1.Orders.Single());

                Assert.Same(customer2, order2.Customer);
                Assert.Same(order2, customer2.Orders.Single());

                Assert.Same(customer3, order3.Customer);
                Assert.Same(order3, customer3.Orders.Single());

                var orderLine1 = context.OrderLines.Single(e => e.Quantity == 7);
                var orderLine2 = context.OrderLines.Single(e => e.Quantity == 1);
                var orderLine3 = context.OrderLines.Single(e => e.Quantity == 2);
                var orderLine4 = context.OrderLines.Single(e => e.Quantity == 3);
                var orderLine5 = context.OrderLines.Single(e => e.Quantity == 4);
                var orderLine6 = context.OrderLines.Single(e => e.Quantity == 5);

                Assert.Same(product1, orderLine1.Product);
                Assert.Same(product2, orderLine2.Product);
                Assert.Same(product3, orderLine3.Product);
                Assert.Same(product2, orderLine4.Product);
                Assert.Same(product1, orderLine5.Product);
                Assert.Same(product2, orderLine6.Product);

                Assert.Same(order1, orderLine1.Order);
                Assert.Same(order1, orderLine2.Order);
                Assert.Same(order2, orderLine3.Order);
                Assert.Same(order2, orderLine4.Order);
                Assert.Same(order2, orderLine5.Order);
                Assert.Same(order3, orderLine6.Order);

                Assert.Equal(
                    new[] { orderLine2, orderLine1 },
                    order1.OrderLines.OrderBy(e => e.Quantity).ToArray());

                Assert.Equal(
                    new[] { orderLine3, orderLine4, orderLine5 },
                    order2.OrderLines.OrderBy(e => e.Quantity).ToArray());

                Assert.Same(orderLine6, order3.OrderLines.Single());

                var productDetail1 = context.ProductDetails.Single(e => e.Details.StartsWith("A"));
                var productDetail2 = context.ProductDetails.Single(e => e.Details.StartsWith("Eeky"));

                Assert.Same(product1, productDetail1.Product);
                Assert.Same(productDetail1, product1.Detail);

                Assert.Same(product2, productDetail2.Product);
                Assert.Same(productDetail2, product2.Detail);

                var productReview1 = context.ProductReviews.Single(e => e.Review.StartsWith("Better"));
                var productReview2 = context.ProductReviews.Single(e => e.Review.StartsWith("Good"));
                var productReview3 = context.ProductReviews.Single(e => e.Review.StartsWith("Eeky"));

                Assert.Same(product1, productReview1.Product);
                Assert.Same(product1, productReview2.Product);
                Assert.Equal(
                    new[] { productReview1, productReview2 },
                    product1.Reviews.OrderBy(r => r.Review).ToArray());

                Assert.Same(product2, productReview3.Product);
                Assert.Same(productReview3, product2.Reviews.Single());

                Assert.Empty(product3.Reviews);

                var productPhoto1 = context.ProductPhotos.Single(e => e.Photo[0] == 101);
                var productPhoto2 = context.ProductPhotos.Single(e => e.Photo[0] == 103);
                var productPhoto3 = context.ProductPhotos.Single(e => e.Photo[0] == 105);

                Assert.Equal(
                    new[] { productPhoto1, productPhoto2 },
                    product1.Photos.OrderBy(r => r.Photo.First()).ToArray());

                Assert.Same(productPhoto3, product3.Photos.Single());
                Assert.Empty(product2.Photos);

                var productWebFeature1 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("Waffle"));
                var productWebFeature2 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("What"));

                Assert.Same(productPhoto1, productWebFeature1.Photo);
                Assert.Same(productWebFeature1, productPhoto1.Features.Single());

                Assert.Same(productReview1, productWebFeature1.Review);
                Assert.Same(productWebFeature1, productReview1.Features.Single());

                Assert.Null(productWebFeature2.Photo);
                Assert.Empty(productPhoto2.Features);

                Assert.Same(productReview3, productWebFeature2.Review);
                Assert.Same(productWebFeature2, productReview3.Features.Single());

                Assert.Empty(productPhoto3.Features);
                Assert.Empty(productReview2.Features);

                var supplier1 = context.Suppliers.Single(e => e.Name.StartsWith("Trading"));
                var supplier2 = context.Suppliers.Single(e => e.Name.StartsWith("Ants"));

                var supplierLogo1 = context.SupplierLogos.Single(e => e.Logo[0] == 201);

                Assert.Same(supplierLogo1, supplier1.Logo);

                var supplierInfo1 = context.SupplierInformation.Single(e => e.Information.StartsWith("Seems"));
                var supplierInfo2 = context.SupplierInformation.Single(e => e.Information.StartsWith("Orange"));
                var supplierInfo3 = context.SupplierInformation.Single(e => e.Information.StartsWith("Very"));

                Assert.Same(supplier1, supplierInfo1.Supplier);
                Assert.Same(supplier1, supplierInfo2.Supplier);
                Assert.Same(supplier2, supplierInfo3.Supplier);

                var customerInfo1 = context.CustomerInformation.Single(e => e.Information.StartsWith("Really"));
                var customerInfo2 = context.CustomerInformation.Single(e => e.Information.StartsWith("Mrs"));

                Assert.Same(customerInfo1, customer1.Info);
                Assert.Same(customerInfo2, customer2.Info);

                var computer1 = context.Computers.Single(e => e.Name == "markash420");
                var computer2 = context.Computers.Single(e => e.Name == "unicorns420");

                var computerDetail1 = context.ComputerDetails.Single(e => e.Specifications == "It's a Dell!");
                var computerDetail2 = context.ComputerDetails.Single(e => e.Specifications == "It's not a Dell!");

                Assert.Same(computer1, computerDetail1.Computer);
                Assert.Same(computerDetail1, computer1.ComputerDetail);

                Assert.Same(computer2, computerDetail2.Computer);
                Assert.Same(computerDetail2, computer2.ComputerDetail);

                var driver1 = context.Drivers.Single(e => e.Name == "Eeky Bear");
                var driver2 = context.Drivers.Single(e => e.Name == "Splash Bear");

                // TODO: Currently these LINQ queries throw InvalidCastException
                //var license1 = context.Licenses.Single(e => e.LicenseNumber == "10");
                //var license2 = context.Licenses.Single(e => e.LicenseNumber == "11");

                //Assert.Same(driver1, license1.Driver);
                //Assert.Same(license1, driver1.License);

                //Assert.Same(driver2, license2.Driver);
                //Assert.Same(license2, driver2.License);
            }
        }

        protected abstract IServiceProvider CreateServiceProvider();

        protected abstract DbContextOptions CreateOptions(string databaseName);

        protected abstract Task CreateAndSeedDatabase(string databaseName, Func<MonsterContext> createContext);

        private SnapshotMonsterContext CreateSnapshotMonsterContext(IServiceProvider serviceProvider)
        {
            return new SnapshotMonsterContext(serviceProvider, CreateOptions(SnapshotDatabaseName));
        }

        private ChangedChangingMonsterContext CreateChangedChangingMonsterContext(IServiceProvider serviceProvider)
        {
            return new ChangedChangingMonsterContext(serviceProvider, CreateOptions(FullNotifyDatabaseName));
        }

        private ChangedOnlyMonsterContext CreateChangedOnlyMonsterContext(IServiceProvider serviceProvider)
        {
            return new ChangedOnlyMonsterContext(serviceProvider, CreateOptions(ChangedOnlyDatabaseName));
        }
    }
}