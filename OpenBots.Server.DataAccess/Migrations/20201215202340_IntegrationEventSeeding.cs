using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class IntegrationEventSeeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "IntegrationEvents",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeleteOn", "DeletedBy", "Description", "EntityType", "IsDeleted", "IsSystem", "Name", "PayloadSchema", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("744ba6f9-161f-41dc-b76e-c1602fc65d1b"), "", null, null, "", "A Queue has been updated", "Queue", false, true, "Queues.QueueUpdated", null, null, null },
                    { new Guid("6ce8b3da-0373-4da2-bc77-ea845212855d"), "", null, null, "", "A new agent has been created", "Agent", false, true, "Agents.NewAgentCreated", null, null, null },
                    { new Guid("35fd2aa3-6c77-4995-9ed8-9b262e5afdfc"), "", null, null, "", "An Agent has reported an unhealthy status", "Agent", false, true, "Agents.UnhealthyReported", null, null, null },
                    { new Guid("6e0c741c-34b0-471e-a491-c7ec61782e94"), "", null, null, "", "An Asset has been deleted", "Asset", false, true, "Assets.AssetDeleted", null, null, null },
                    { new Guid("4ce67735-2edc-4b7f-849a-5575740a496f"), "", null, null, "", "An Asset has been updated", "Asset", false, true, "Assets.AssetUpdated", null, null, null },
                    { new Guid("f1b111cc-1f26-404d-827c-e30305c2ecc4"), "", null, null, "", "A new Asset has been created", "Asset", false, true, "Assets.NewAssetCreated", null, null, null },
                    { new Guid("90f9f691-90e5-41d0-9b2c-1e8437bc85d3"), "", null, null, "", "A Process has been deleted", "Automation", false, true, "Automations.AutomationDeleted", null, null, null },
                    { new Guid("8437fa1f-777a-4905-a169-feb32214c0c8"), "", null, null, "", "A Process has been updated", "Automation", false, true, "Automations.AutomationUpdated", null, null, null },
                    { new Guid("93416738-3284-4bb0-869e-e2f191446b44"), "", null, null, "", "A new Process has been created", "Automation", false, true, "Automations.NewAutomationCreated", null, null, null },
                    { new Guid("ecced501-9c35-4b37-a7b2-b6b901f91234"), "", null, null, "", "A Credential has been deleted", "Credential", false, true, "Credentials.CredentialDeleted", null, null, null },
                    { new Guid("efd1d688-1881-4d5e-aed7-81528d54d7ef"), "", null, null, "", "A Credential has been updated", "Credential", false, true, "Credentials.CredentialUpdated", null, null, null },
                    { new Guid("2b4bd195-62ac-4111-97ca-d6df6dd3f0fb"), "", null, null, "", "An Agent has been updated", "Agent", false, true, "Agents.AgentUpdated", null, null, null },
                    { new Guid("76f6ab13-c430-46ad-b859-3d2dfd802e84"), "", null, null, "", "A new Credential has been created", "Credential", false, true, "Credentials.NewCredentialCreated", null, null, null },
                    { new Guid("3ff9b456-7832-4499-b263-692c021e7d80"), "", null, null, "", "A File has been updated", "File", false, true, "Files.FileUpdated", null, null, null },
                    { new Guid("04cf6a7a-ca72-48bc-887f-666ef580d198"), "", null, null, "", "A new File has been created", "File", false, true, "Files.NewFileCreated", null, null, null },
                    { new Guid("82b8d08d-5ae2-4031-bdf8-5fba5597ac4b"), "", null, null, "", "A Job has been deleted", "Job", false, true, "Jobs.JobsDeleted", null, null, null },
                    { new Guid("9d8e576a-a69d-43cf-bbc9-18103105d0a0"), "", null, null, "", "A Job has been updated", "Job", false, true, "Jobs.JobUpdated", null, null, null },
                    { new Guid("06dd9940-a483-4a21-9551-cf2e32eeccae"), "", null, null, "", "A new Job has been created", "Job", false, true, "Jobs.NewJobCreated", null, null, null },
                    { new Guid("30a8dcb9-10cf-43c6-a08f-b45fe2125dae"), "", null, null, "", "A new QueueItem has been created", "QueueItem", false, true, "QueueItems.NewQueueItemCreated", null, null, null },
                    { new Guid("860689af-fd19-44ba-a5c7-53f6fed92065"), "", null, null, "", "A QueueItem has been deleted", "QueueItem", false, true, "QueueItems.QueueItemDeleted", null, null, null },
                    { new Guid("0719a4c3-2143-4b9a-92ae-8b5a93075b98"), "", null, null, "", "A QueueItem has been updated", "QueueItem", false, true, "QueueItems.QueueItemUpdated", null, null, null },
                    { new Guid("e9f64119-edbf-4779-a796-21ad59f76534"), "", null, null, "", "A new Queue has been created", "Queue", false, true, "Queues.NewQueueCreated", null, null, null },
                    { new Guid("b00eeecd-5729-4f82-9cd2-dcfafd946965"), "", null, null, "", "A Queue has been deleted", "Queue", false, true, "Queues.QueueDeleted", null, null, null },
                    { new Guid("32d63e9d-aa6e-481f-b928-541ddf979bdf"), "", null, null, "", "A File has been deleted", "File", false, true, "Files.FileDeleted", null, null, null },
                    { new Guid("6ce0bb0e-cda1-49fa-a9e4-b67d904f826e"), "", null, null, "", "An Agent has been deleted", "Agent", false, true, "Agents.AgentDeleted", null, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("04cf6a7a-ca72-48bc-887f-666ef580d198"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("06dd9940-a483-4a21-9551-cf2e32eeccae"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("0719a4c3-2143-4b9a-92ae-8b5a93075b98"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("2b4bd195-62ac-4111-97ca-d6df6dd3f0fb"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("30a8dcb9-10cf-43c6-a08f-b45fe2125dae"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("32d63e9d-aa6e-481f-b928-541ddf979bdf"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("35fd2aa3-6c77-4995-9ed8-9b262e5afdfc"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("3ff9b456-7832-4499-b263-692c021e7d80"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("4ce67735-2edc-4b7f-849a-5575740a496f"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("6ce0bb0e-cda1-49fa-a9e4-b67d904f826e"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("6ce8b3da-0373-4da2-bc77-ea845212855d"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("6e0c741c-34b0-471e-a491-c7ec61782e94"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("744ba6f9-161f-41dc-b76e-c1602fc65d1b"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("76f6ab13-c430-46ad-b859-3d2dfd802e84"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("82b8d08d-5ae2-4031-bdf8-5fba5597ac4b"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("8437fa1f-777a-4905-a169-feb32214c0c8"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("860689af-fd19-44ba-a5c7-53f6fed92065"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("90f9f691-90e5-41d0-9b2c-1e8437bc85d3"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("93416738-3284-4bb0-869e-e2f191446b44"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("9d8e576a-a69d-43cf-bbc9-18103105d0a0"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("b00eeecd-5729-4f82-9cd2-dcfafd946965"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("e9f64119-edbf-4779-a796-21ad59f76534"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("ecced501-9c35-4b37-a7b2-b6b901f91234"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("efd1d688-1881-4d5e-aed7-81528d54d7ef"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("f1b111cc-1f26-404d-827c-e30305c2ecc4"));
        }
    }
}
