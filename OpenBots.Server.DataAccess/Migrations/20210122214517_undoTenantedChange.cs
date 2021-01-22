using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class undoTenantedChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "UserAgreements");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ScheduleParameters");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Queues");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "QueueItemAttachments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "PersonEmails");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "PersonCredentials");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "PasswordPolicies");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "JobParameters");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "JobCheckpoints");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IntegrationEventSubscriptions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IntegrationEventSubscriptionAttempts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IntegrationEvents");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IntegrationEventLogs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "EmailVerifications");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "EmailAttachments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "EmailAccounts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AutomationVersions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Automations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AutomationLogs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AutomationExecutionLogs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AppVersion");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AgentHeartbeats");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "UserConsents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "UserAgreements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Schedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "ScheduleParameters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Queues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "QueueItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "QueueItemAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "PersonEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "PersonCredentials",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "PasswordPolicies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "JobParameters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "JobCheckpoints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "IntegrationEventSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "IntegrationEventSubscriptionAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "IntegrationEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "IntegrationEventLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "EmailVerifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Emails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "EmailAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "EmailAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Credentials",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "ConfigurationValues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AutomationVersions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Automations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AutomationLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AutomationExecutionLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Assets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AppVersion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Agents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AgentHeartbeats",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
