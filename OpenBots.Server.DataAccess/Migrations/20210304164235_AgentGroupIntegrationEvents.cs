using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AgentGroupIntegrationEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "IntegrationEvents",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeleteOn", "DeletedBy", "Description", "EntityType", "IsDeleted", "IsSystem", "Name", "PayloadSchema", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("2c5b29c7-2fed-42b6-afcb-b7d8a41aacb5"), "", null, null, "", "A new AgentGroup has been created", "AgentGroup", false, true, "AgentGroups.NewAgentGroupCreated", null, null, null },
                    { new Guid("e096bb0f-850c-4001-946a-88a7f8692d5a"), "", null, null, "", "An AgentGroup has been updated", "AgentGroup", false, true, "AgentGroups.AgentGroupUpdated", null, null, null },
                    { new Guid("3789f1ae-2693-4ad7-8696-723bd551199f"), "", null, null, "", "An AgentGroup has been deleted", "AgentGroup", false, true, "AgentGroups.AgentGroupDeleted", null, null, null },
                    { new Guid("76910164-6fda-4861-b1b5-7737370a8461"), "", null, null, "", "An Agent has been added to the AgentGroup", "AgentGroup", false, true, "AgentGroups.AgentGroupMemberCreated", null, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("2c5b29c7-2fed-42b6-afcb-b7d8a41aacb5"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("3789f1ae-2693-4ad7-8696-723bd551199f"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("76910164-6fda-4861-b1b5-7737370a8461"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("e096bb0f-850c-4001-946a-88a7f8692d5a"));
        }
    }
}
