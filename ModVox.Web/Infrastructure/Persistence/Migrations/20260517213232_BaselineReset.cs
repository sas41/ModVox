using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModVox.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BaselineReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    must_change_credentials = table.Column<bool>(type: "boolean", nullable: false),
                    ban_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ban_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    session_version = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_sessions",
                columns: table => new
                {
                    session_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_account_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    event_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_log_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "mods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintainer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    owner = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    repository = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    default_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    readme_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    changelog_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    images_folder = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    readme_markdown = table.Column<string>(type: "text", nullable: true),
                    readme_html = table.Column<string>(type: "text", nullable: true),
                    changelog_markdown = table.Column<string>(type: "text", nullable: true),
                    changelog_html = table.Column<string>(type: "text", nullable: true),
                    content_fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tag_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    credits = table.Column<string>(type: "jsonb", nullable: false),
                    external_credits = table.Column<string>(type: "jsonb", nullable: false),
                    download_count = table.Column<long>(type: "bigint", nullable: false),
                    moderation_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    verify_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    key_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    key_version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_accepted_refresh_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mods", x => x.id);
                    table.ForeignKey(
                        name: "FK_mods_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mods_users_maintainer_user_id",
                        column: x => x.maintainer_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mod_releases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mod_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_prerelease = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_releases", x => x.id);
                    table.ForeignKey(
                        name: "FK_mod_releases_mods_mod_id",
                        column: x => x.mod_id,
                        principalTable: "mods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mod_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reporter_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    resolved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolution_note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_mod_reports_mods_mod_id",
                        column: x => x.mod_id,
                        principalTable: "mods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mod_reports_users_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mod_reports_users_resolved_by_user_id",
                        column: x => x.resolved_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "refresh_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mod_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    owner = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    repository = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    @ref = table.Column<string>(name: "ref", type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    result = table.Column<string>(type: "text", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    enqueued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_jobs_mods_mod_id",
                        column: x => x.mod_id,
                        principalTable: "mods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_release_artifacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    release_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    download_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_release_artifacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_mod_release_artifacts_mod_releases_release_id",
                        column: x => x.release_id,
                        principalTable: "mod_releases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_sessions_expires_at",
                table: "account_sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_account_sessions_user_id",
                table: "account_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_actor_user_id",
                table: "audit_log",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_created_at",
                table: "audit_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_event_type",
                table: "audit_log",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_games_slug",
                table: "games",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mod_release_artifacts_download_url",
                table: "mod_release_artifacts",
                column: "download_url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mod_release_artifacts_release_id",
                table: "mod_release_artifacts",
                column: "release_id");

            migrationBuilder.CreateIndex(
                name: "ix_mod_releases_mod_id",
                table: "mod_releases",
                column: "mod_id");

            migrationBuilder.CreateIndex(
                name: "ix_mod_releases_mod_tag",
                table: "mod_releases",
                columns: new[] { "mod_id", "tag_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mod_reports_mod_id",
                table: "mod_reports",
                column: "mod_id");

            migrationBuilder.CreateIndex(
                name: "IX_mod_reports_reporter_user_id",
                table: "mod_reports",
                column: "reporter_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mod_reports_resolved_by_user_id",
                table: "mod_reports",
                column: "resolved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_mod_reports_status",
                table: "mod_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_mods_coordinates",
                table: "mods",
                columns: new[] { "provider", "owner", "repository" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mods_game_id",
                table: "mods",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_mods_key_hash",
                table: "mods",
                column: "key_hash");

            migrationBuilder.CreateIndex(
                name: "ix_mods_maintainer_user_id",
                table: "mods",
                column: "maintainer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_mods_moderation_status",
                table: "mods",
                column: "moderation_status");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_jobs_mod_id",
                table: "refresh_jobs",
                column: "mod_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_jobs_mod_idempotency",
                table: "refresh_jobs",
                columns: new[] { "mod_id", "idempotency_key" });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_jobs_status",
                table: "refresh_jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tags_label",
                table: "tags",
                column: "label",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_sessions");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "mod_release_artifacts");

            migrationBuilder.DropTable(
                name: "mod_reports");

            migrationBuilder.DropTable(
                name: "refresh_jobs");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "mod_releases");

            migrationBuilder.DropTable(
                name: "mods");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
