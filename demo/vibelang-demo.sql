-- VibeLang demo database (content only)
-- Generated from the live vibelang_db on 2026-11-08.
-- Excludes every AspNet* user table and all per-user progress rows, so no
-- real accounts, e-mails or password hashes are published. The application
-- seeds its own demo admin (admin@vibelang.com) and the Admin/User roles on
-- first start; DbInitializer sees the existing courses and only backfills
-- achievements if they are missing.

--
-- PostgreSQL database dump
--


-- Dumped from database version 17.11 (Debian 17.11-0+deb13u1)
-- Dumped by pg_dump version 17.11 (Debian 17.11-0+deb13u1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public."VocabularyWords" DROP CONSTRAINT IF EXISTS "FK_VocabularyWords_Lessons_LessonId";
ALTER TABLE IF EXISTS ONLY public."UserVocabularies" DROP CONSTRAINT IF EXISTS "FK_UserVocabularies_VocabularyWords_WordId";
ALTER TABLE IF EXISTS ONLY public."UserVocabularies" DROP CONSTRAINT IF EXISTS "FK_UserVocabularies_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."UserLessonProgresses" DROP CONSTRAINT IF EXISTS "FK_UserLessonProgresses_Lessons_LessonId";
ALTER TABLE IF EXISTS ONLY public."UserLessonProgresses" DROP CONSTRAINT IF EXISTS "FK_UserLessonProgresses_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."UserCourseStats" DROP CONSTRAINT IF EXISTS "FK_UserCourseStats_Courses_CourseId";
ALTER TABLE IF EXISTS ONLY public."UserCourseStats" DROP CONSTRAINT IF EXISTS "FK_UserCourseStats_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."UserAchievements" DROP CONSTRAINT IF EXISTS "FK_UserAchievements_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."UserAchievements" DROP CONSTRAINT IF EXISTS "FK_UserAchievements_Achievements_AchievementId";
ALTER TABLE IF EXISTS ONLY public."Quizzes" DROP CONSTRAINT IF EXISTS "FK_Quizzes_Lessons_LessonId";
ALTER TABLE IF EXISTS ONLY public."QuizQuestions" DROP CONSTRAINT IF EXISTS "FK_QuizQuestions_Quizzes_QuizId";
ALTER TABLE IF EXISTS ONLY public."QuizOptions" DROP CONSTRAINT IF EXISTS "FK_QuizOptions_QuizQuestions_QuestionId";
ALTER TABLE IF EXISTS ONLY public."Lessons" DROP CONSTRAINT IF EXISTS "FK_Lessons_Chapters_ChapterId";
ALTER TABLE IF EXISTS ONLY public."Chapters" DROP CONSTRAINT IF EXISTS "FK_Chapters_Courses_CourseId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS "FK_AspNetUserTokens_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "FK_AspNetUserRoles_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "FK_AspNetUserRoles_AspNetRoles_RoleId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS "FK_AspNetUserLogins_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS "FK_AspNetUserClaims_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS "FK_AspNetRoleClaims_AspNetRoles_RoleId";
DROP INDEX IF EXISTS public."UserNameIndex";
DROP INDEX IF EXISTS public."RoleNameIndex";
DROP INDEX IF EXISTS public."IX_VocabularyWords_LessonId_Word";
DROP INDEX IF EXISTS public."IX_UserVocabularies_WordId";
DROP INDEX IF EXISTS public."IX_UserVocabularies_UserId_WordId";
DROP INDEX IF EXISTS public."IX_UserLessonProgresses_UserId";
DROP INDEX IF EXISTS public."IX_UserLessonProgresses_LessonId";
DROP INDEX IF EXISTS public."IX_UserCourseStats_UserId";
DROP INDEX IF EXISTS public."IX_UserCourseStats_CourseId";
DROP INDEX IF EXISTS public."IX_UserAchievements_UserId";
DROP INDEX IF EXISTS public."IX_UserAchievements_AchievementId";
DROP INDEX IF EXISTS public."IX_Quizzes_LessonId";
DROP INDEX IF EXISTS public."IX_QuizQuestions_QuizId";
DROP INDEX IF EXISTS public."IX_QuizOptions_QuestionId";
DROP INDEX IF EXISTS public."IX_Lessons_ChapterId";
DROP INDEX IF EXISTS public."IX_Chapters_CourseId";
DROP INDEX IF EXISTS public."IX_AspNetUserRoles_RoleId";
DROP INDEX IF EXISTS public."IX_AspNetUserLogins_UserId";
DROP INDEX IF EXISTS public."IX_AspNetUserClaims_UserId";
DROP INDEX IF EXISTS public."IX_AspNetRoleClaims_RoleId";
DROP INDEX IF EXISTS public."EmailIndex";
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
ALTER TABLE IF EXISTS ONLY public."VocabularyWords" DROP CONSTRAINT IF EXISTS "PK_VocabularyWords";
ALTER TABLE IF EXISTS ONLY public."UserVocabularies" DROP CONSTRAINT IF EXISTS "PK_UserVocabularies";
ALTER TABLE IF EXISTS ONLY public."UserLessonProgresses" DROP CONSTRAINT IF EXISTS "PK_UserLessonProgresses";
ALTER TABLE IF EXISTS ONLY public."UserCourseStats" DROP CONSTRAINT IF EXISTS "PK_UserCourseStats";
ALTER TABLE IF EXISTS ONLY public."UserAchievements" DROP CONSTRAINT IF EXISTS "PK_UserAchievements";
ALTER TABLE IF EXISTS ONLY public."Quizzes" DROP CONSTRAINT IF EXISTS "PK_Quizzes";
ALTER TABLE IF EXISTS ONLY public."QuizQuestions" DROP CONSTRAINT IF EXISTS "PK_QuizQuestions";
ALTER TABLE IF EXISTS ONLY public."QuizOptions" DROP CONSTRAINT IF EXISTS "PK_QuizOptions";
ALTER TABLE IF EXISTS ONLY public."Lessons" DROP CONSTRAINT IF EXISTS "PK_Lessons";
ALTER TABLE IF EXISTS ONLY public."Courses" DROP CONSTRAINT IF EXISTS "PK_Courses";
ALTER TABLE IF EXISTS ONLY public."Chapters" DROP CONSTRAINT IF EXISTS "PK_Chapters";
ALTER TABLE IF EXISTS ONLY public."AspNetUsers" DROP CONSTRAINT IF EXISTS "PK_AspNetUsers";
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS "PK_AspNetUserTokens";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "PK_AspNetUserRoles";
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS "PK_AspNetUserLogins";
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS "PK_AspNetUserClaims";
ALTER TABLE IF EXISTS ONLY public."AspNetRoles" DROP CONSTRAINT IF EXISTS "PK_AspNetRoles";
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS "PK_AspNetRoleClaims";
ALTER TABLE IF EXISTS ONLY public."Achievements" DROP CONSTRAINT IF EXISTS "PK_Achievements";
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP TABLE IF EXISTS public."VocabularyWords";
DROP TABLE IF EXISTS public."UserVocabularies";
DROP TABLE IF EXISTS public."UserLessonProgresses";
DROP TABLE IF EXISTS public."UserCourseStats";
DROP TABLE IF EXISTS public."UserAchievements";
DROP TABLE IF EXISTS public."Quizzes";
DROP TABLE IF EXISTS public."QuizQuestions";
DROP TABLE IF EXISTS public."QuizOptions";
DROP TABLE IF EXISTS public."Lessons";
DROP TABLE IF EXISTS public."Courses";
DROP TABLE IF EXISTS public."Chapters";
DROP TABLE IF EXISTS public."AspNetUsers";
DROP TABLE IF EXISTS public."AspNetUserTokens";
DROP TABLE IF EXISTS public."AspNetUserRoles";
DROP TABLE IF EXISTS public."AspNetUserLogins";
DROP TABLE IF EXISTS public."AspNetUserClaims";
DROP TABLE IF EXISTS public."AspNetRoles";
DROP TABLE IF EXISTS public."AspNetRoleClaims";
DROP TABLE IF EXISTS public."Achievements";
-- *not* dropping schema, since initdb creates it
--
-- Name: public; Type: SCHEMA; Schema: -; Owner: -
--

-- *not* creating schema, since initdb creates it


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Achievements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Achievements" (
    "Id" integer NOT NULL,
    "Title" text NOT NULL,
    "Description" text NOT NULL,
    "IconPath" text
);


--
-- Name: Achievements_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Achievements" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Achievements_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetRoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoleClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetRoles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetRoles" (
    "Id" text NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text
);


--
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserClaims" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUserClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" text NOT NULL
);


--
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL
);


--
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text
);


--
-- Name: AspNetUsers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUsers" (
    "Id" text NOT NULL,
    "LastName" text,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "FirstName" text,
    "ProfilePicturePath" text
);


--
-- Name: Chapters; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Chapters" (
    "Id" integer NOT NULL,
    "Title" character varying(150) NOT NULL,
    "Order" integer NOT NULL,
    "CourseId" integer NOT NULL
);


--
-- Name: Chapters_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Chapters" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Chapters_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Courses; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Courses" (
    "Id" integer NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" character varying(500),
    "IsoCode" text DEFAULT ''::text NOT NULL
);


--
-- Name: Courses_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Courses" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Courses_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Lessons; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Lessons" (
    "Id" integer NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Order" integer NOT NULL,
    "ChapterId" integer NOT NULL,
    "Difficulty" text,
    "ContentJson" text
);


--
-- Name: Lessons_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Lessons" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Lessons_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: QuizOptions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."QuizOptions" (
    "Id" integer NOT NULL,
    "OptionText" text NOT NULL,
    "IsCorrect" boolean NOT NULL,
    "QuestionId" integer NOT NULL
);


--
-- Name: QuizOptions_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."QuizOptions" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."QuizOptions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: QuizQuestions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."QuizQuestions" (
    "Id" integer NOT NULL,
    "QuestionText" character varying(500) NOT NULL,
    "QuizId" integer NOT NULL,
    "CorrectAnswer" character varying(200),
    "MatchingDataJson" text,
    "Tip" integer DEFAULT 0 NOT NULL
);


--
-- Name: QuizQuestions_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."QuizQuestions" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."QuizQuestions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Quizzes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Quizzes" (
    "Id" integer NOT NULL,
    "Title" character varying(150) NOT NULL,
    "LessonId" integer NOT NULL
);


--
-- Name: Quizzes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Quizzes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Quizzes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: UserAchievements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserAchievements" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "AchievementId" integer NOT NULL,
    "EarnedDate" timestamp with time zone NOT NULL
);


--
-- Name: UserAchievements_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."UserAchievements" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."UserAchievements_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: UserCourseStats; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserCourseStats" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "CourseId" integer NOT NULL,
    "TotalXP" integer NOT NULL,
    "CurrentStreak" integer NOT NULL,
    "LastActivityDate" timestamp with time zone,
    "MaxStreakEver" integer DEFAULT 0 NOT NULL,
    "StreakBrokenToday" boolean DEFAULT false NOT NULL
);


--
-- Name: UserCourseStats_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."UserCourseStats" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."UserCourseStats_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: UserLessonProgresses; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserLessonProgresses" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "LessonId" integer NOT NULL,
    "IsCompleted" boolean NOT NULL,
    "ScoreAchieved" integer NOT NULL,
    "CompletionDate" timestamp with time zone NOT NULL
);


--
-- Name: UserLessonProgresses_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."UserLessonProgresses" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."UserLessonProgresses_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: UserVocabularies; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserVocabularies" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "WordId" integer NOT NULL,
    "Status" text NOT NULL,
    "LastReviewed" timestamp with time zone NOT NULL
);


--
-- Name: UserVocabularies_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."UserVocabularies" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."UserVocabularies_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: VocabularyWords; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."VocabularyWords" (
    "Id" integer NOT NULL,
    "Word" character varying(100) NOT NULL,
    "Translation" character varying(100) NOT NULL,
    "ExampleSentence" character varying(300),
    "LessonId" integer NOT NULL
);


--
-- Name: VocabularyWords_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."VocabularyWords" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."VocabularyWords_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Name: Achievements PK_Achievements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Achievements"
    ADD CONSTRAINT "PK_Achievements" PRIMARY KEY ("Id");


--
-- Name: AspNetRoleClaims PK_AspNetRoleClaims; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetRoles PK_AspNetRoles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoles"
    ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");


--
-- Name: AspNetUserClaims PK_AspNetUserClaims; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetUserLogins PK_AspNetUserLogins; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");


--
-- Name: AspNetUserRoles PK_AspNetUserRoles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");


--
-- Name: AspNetUserTokens PK_AspNetUserTokens; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");


--
-- Name: AspNetUsers PK_AspNetUsers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");


--
-- Name: Chapters PK_Chapters; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Chapters"
    ADD CONSTRAINT "PK_Chapters" PRIMARY KEY ("Id");


--
-- Name: Courses PK_Courses; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Courses"
    ADD CONSTRAINT "PK_Courses" PRIMARY KEY ("Id");


--
-- Name: Lessons PK_Lessons; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Lessons"
    ADD CONSTRAINT "PK_Lessons" PRIMARY KEY ("Id");


--
-- Name: QuizOptions PK_QuizOptions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."QuizOptions"
    ADD CONSTRAINT "PK_QuizOptions" PRIMARY KEY ("Id");


--
-- Name: QuizQuestions PK_QuizQuestions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."QuizQuestions"
    ADD CONSTRAINT "PK_QuizQuestions" PRIMARY KEY ("Id");


--
-- Name: Quizzes PK_Quizzes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Quizzes"
    ADD CONSTRAINT "PK_Quizzes" PRIMARY KEY ("Id");


--
-- Name: UserAchievements PK_UserAchievements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserAchievements"
    ADD CONSTRAINT "PK_UserAchievements" PRIMARY KEY ("Id");


--
-- Name: UserCourseStats PK_UserCourseStats; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserCourseStats"
    ADD CONSTRAINT "PK_UserCourseStats" PRIMARY KEY ("Id");


--
-- Name: UserLessonProgresses PK_UserLessonProgresses; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserLessonProgresses"
    ADD CONSTRAINT "PK_UserLessonProgresses" PRIMARY KEY ("Id");


--
-- Name: UserVocabularies PK_UserVocabularies; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserVocabularies"
    ADD CONSTRAINT "PK_UserVocabularies" PRIMARY KEY ("Id");


--
-- Name: VocabularyWords PK_VocabularyWords; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."VocabularyWords"
    ADD CONSTRAINT "PK_VocabularyWords" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- Name: IX_AspNetRoleClaims_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");


--
-- Name: IX_AspNetUserClaims_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");


--
-- Name: IX_AspNetUserLogins_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");


--
-- Name: IX_AspNetUserRoles_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");


--
-- Name: IX_Chapters_CourseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Chapters_CourseId" ON public."Chapters" USING btree ("CourseId");


--
-- Name: IX_Lessons_ChapterId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Lessons_ChapterId" ON public."Lessons" USING btree ("ChapterId");


--
-- Name: IX_QuizOptions_QuestionId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_QuizOptions_QuestionId" ON public."QuizOptions" USING btree ("QuestionId");


--
-- Name: IX_QuizQuestions_QuizId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_QuizQuestions_QuizId" ON public."QuizQuestions" USING btree ("QuizId");


--
-- Name: IX_Quizzes_LessonId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Quizzes_LessonId" ON public."Quizzes" USING btree ("LessonId");


--
-- Name: IX_UserAchievements_AchievementId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserAchievements_AchievementId" ON public."UserAchievements" USING btree ("AchievementId");


--
-- Name: IX_UserAchievements_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserAchievements_UserId" ON public."UserAchievements" USING btree ("UserId");


--
-- Name: IX_UserCourseStats_CourseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserCourseStats_CourseId" ON public."UserCourseStats" USING btree ("CourseId");


--
-- Name: IX_UserCourseStats_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserCourseStats_UserId" ON public."UserCourseStats" USING btree ("UserId");


--
-- Name: IX_UserLessonProgresses_LessonId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserLessonProgresses_LessonId" ON public."UserLessonProgresses" USING btree ("LessonId");


--
-- Name: IX_UserLessonProgresses_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserLessonProgresses_UserId" ON public."UserLessonProgresses" USING btree ("UserId");


--
-- Name: IX_UserVocabularies_UserId_WordId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_UserVocabularies_UserId_WordId" ON public."UserVocabularies" USING btree ("UserId", "WordId");


--
-- Name: IX_UserVocabularies_WordId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserVocabularies_WordId" ON public."UserVocabularies" USING btree ("WordId");


--
-- Name: IX_VocabularyWords_LessonId_Word; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_VocabularyWords_LessonId_Word" ON public."VocabularyWords" USING btree ("LessonId", "Word");


--
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");


--
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");


--
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: Chapters FK_Chapters_Courses_CourseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Chapters"
    ADD CONSTRAINT "FK_Chapters_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES public."Courses"("Id") ON DELETE CASCADE;


--
-- Name: Lessons FK_Lessons_Chapters_ChapterId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Lessons"
    ADD CONSTRAINT "FK_Lessons_Chapters_ChapterId" FOREIGN KEY ("ChapterId") REFERENCES public."Chapters"("Id") ON DELETE CASCADE;


--
-- Name: QuizOptions FK_QuizOptions_QuizQuestions_QuestionId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."QuizOptions"
    ADD CONSTRAINT "FK_QuizOptions_QuizQuestions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES public."QuizQuestions"("Id") ON DELETE CASCADE;


--
-- Name: QuizQuestions FK_QuizQuestions_Quizzes_QuizId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."QuizQuestions"
    ADD CONSTRAINT "FK_QuizQuestions_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES public."Quizzes"("Id") ON DELETE CASCADE;


--
-- Name: Quizzes FK_Quizzes_Lessons_LessonId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Quizzes"
    ADD CONSTRAINT "FK_Quizzes_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES public."Lessons"("Id") ON DELETE CASCADE;


--
-- Name: UserAchievements FK_UserAchievements_Achievements_AchievementId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserAchievements"
    ADD CONSTRAINT "FK_UserAchievements_Achievements_AchievementId" FOREIGN KEY ("AchievementId") REFERENCES public."Achievements"("Id") ON DELETE CASCADE;


--
-- Name: UserAchievements FK_UserAchievements_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserAchievements"
    ADD CONSTRAINT "FK_UserAchievements_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: UserCourseStats FK_UserCourseStats_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserCourseStats"
    ADD CONSTRAINT "FK_UserCourseStats_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: UserCourseStats FK_UserCourseStats_Courses_CourseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserCourseStats"
    ADD CONSTRAINT "FK_UserCourseStats_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES public."Courses"("Id") ON DELETE CASCADE;


--
-- Name: UserLessonProgresses FK_UserLessonProgresses_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserLessonProgresses"
    ADD CONSTRAINT "FK_UserLessonProgresses_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: UserLessonProgresses FK_UserLessonProgresses_Lessons_LessonId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserLessonProgresses"
    ADD CONSTRAINT "FK_UserLessonProgresses_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES public."Lessons"("Id") ON DELETE CASCADE;


--
-- Name: UserVocabularies FK_UserVocabularies_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserVocabularies"
    ADD CONSTRAINT "FK_UserVocabularies_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: UserVocabularies FK_UserVocabularies_VocabularyWords_WordId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserVocabularies"
    ADD CONSTRAINT "FK_UserVocabularies_VocabularyWords_WordId" FOREIGN KEY ("WordId") REFERENCES public."VocabularyWords"("Id") ON DELETE CASCADE;


--
-- Name: VocabularyWords FK_VocabularyWords_Lessons_LessonId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."VocabularyWords"
    ADD CONSTRAINT "FK_VocabularyWords_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES public."Lessons"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--



-- Content rows only; FK checks are deferred for the import.
SET session_replication_role = replica;
--
-- PostgreSQL database dump
--


-- Dumped from database version 17.11 (Debian 17.11-0+deb13u1)
-- Dumped by pg_dump version 17.11 (Debian 17.11-0+deb13u1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Data for Name: Achievements; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Achievements" VALUES
	(1, 'Beginner''s Start', 'Complete your first lesson', '🎯'),
	(2, 'Speed Learner', 'Complete 5 lessons in one day', '⚡'),
	(3, 'Perfect Score', 'Score 100% on a quiz', '💯'),
	(4, 'Marathon Runner', 'Maintain a 7-day streak', '🏃'),
	(5, 'Expert Learner', 'Complete all lessons in a course', '⭐'),
	(6, 'Social Butterfly', 'Reach the top 10 on the leaderboard', '👥');


--
-- Data for Name: Courses; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Courses" VALUES
	(1, 'English (Romanian)', 'Learn the essential phrases and grammar.', 'en'),
	(3, 'Vibelandeza', 'Invata o limba extraterestra', 'VL'),
	(4, 'jitaru', 'exemplu', 'ss'),
	(5, 'French Basics', NULL, 'fr'),
	(2, 'English (Romanian)', 'Ceva curs in engleza cred', 'en');


--
-- Data for Name: Chapters; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Chapters" VALUES
	(3, 'Chapter 3 - Slang', 3, 2),
	(1, 'Chapter 1: Getting Started :)', 1, 1),
	(2, 'blablabla iar', 2, 1);


--
-- Data for Name: Lessons; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Lessons" VALUES
	(3, 'ceva', 1, 2, NULL, '{
  "numeLectie": "Lecția 1: Fraze Comune",
  "dificultate": "Ușor",
  "cuvinteInvatate": ["bună dimineața", "bună seara", "noapte bună", "cum ești?", "sunt bine", "mulțumesc", "cu plăcere", "scuze", "da", "nu"],
  "teste": [
    {
      "tip": 1,
      "propozitie": "Bună dimineața, cum te simți?",
      "raspunsCorect": "Good morning, how do you feel?"
    },
    {
      "tip": 1,
      "propozitie": "Mulțumesc mult pentru ajutor.",
      "raspunsCorect": "Thank you very much for your help."
    },
    {
      "tip": 1,
      "propozitie": "Noapte bună și vise plăcute.",
      "raspunsCorect": "Good night and sweet dreams."
    },
    {
      "tip": 2,
      "propozitie": "How are you today?",
      "raspunsCorect": "Cum ești azi?"
    },
    {
      "tip": 2,
      "propozitie": "You are welcome.",
      "raspunsCorect": "Cu plăcere."
    },
    {
      "tip": 2,
      "propozitie": "Excuse me, where is the exit?",
      "raspunsCorect": "Scuze, unde este ieșirea?"
    },
    {
      "tip": 3,
      "leftWords": ["masă", "scaun", "carte", "apă", "pâine"],
      "rightWords": ["table", "chair", "book", "water", "bread"]
    },
    {
      "tip": 3,
      "leftWords": ["casă", "mașină", "copac", "floare", "soare"],
      "rightWords": ["house", "car", "tree", "flower", "sun"]
    },
    {
      "tip": 4,
      "propozitie": "Un exemplu de folosire a cuvântului ''hello'': ''Hello, how are you?'' (Bună, cum ești?)"
    },
    {
      "tip": 4,
      "propozitie": "Un exemplu de folosire a cuvântului ''yes'': ''Yes, I want to go to the cinema.'' (Da, vreau să merg la film.)"
    },
    {
      "tip": 4,
      "propozitie": "Un exemplu de folosire a cuvântului ''no'': ''No, I cannot come today.'' (Nu, nu pot veni azi.)"
    }
  ]
}'),
	(4, 'Slang (1)', 1, 3, 'Usor', '{
  "numeLectie": "Lecția 1: Slang și Brainrot",
  "dificultate": "Avansat",
  "cuvinteInvatate": ["rizz", "no cap", "sus", "bet", "ghosting", "sigma", "salty", "delulu", "skibidi", "slay"],
  "teste": [
    {
      "tip": 1,
      "propozitie": "Are multă carismă, pe bune.",
      "raspunsCorect": "He has a lot of rizz, no cap."
    },
    {
      "tip": 1,
      "propozitie": "Acel tip este foarte dubios, să știi.",
      "raspunsCorect": "That guy is very sus, you know."
    },
    {
      "tip": 1,
      "propozitie": "Ești ofticat pentru că ai pierdut meciul.",
      "raspunsCorect": "You are salty because you lost the match."
    },
    {
      "tip": 2,
      "propozitie": "She is totally delulu right now.",
      "raspunsCorect": "Ea este complet ruptă de realitate acum."
    },
    {
      "tip": 2,
      "propozitie": "Bet, I will see you tomorrow at school.",
      "raspunsCorect": "S-a făcut, ne vedem mâine la școală."
    },
    {
      "tip": 2,
      "propozitie": "He is a true sigma male.",
      "raspunsCorect": "El este un adevărat mascul bazat (șmecher/independent)."
    },
    {
      "tip": 3,
      "leftWords": ["carismă", "minciună", "dubios", "ofticat", "de acord"],
      "rightWords": ["rizz", "cap", "sus", "salty", "bet"]
    },
    {
      "tip": 3,
      "leftWords": ["rupt de realitate", "a ignora", "super/fabulos", "bazat", "delicios/foarte bun"],
      "rightWords": ["delulu", "ghosting", "slay", "sigma", "bussin"]
    },
    {
      "tip": 4,
      "propozitie": "Un exemplu de folosire a cuvântului ''rizz'': ''He rizzed her up instantly.'' (A vrăjit-o/agățat-o instant cu carisma lui.)"
    },
    {
      "tip": 4,
      "propozitie": "Un exemplu de folosire a expresiei ''no cap'': ''This game is amazing, no cap.'' (Jocul ăsta e incredibil, jur/pe bune.)"
    },
    {
      "tip": 4,
      "propozitie": "Un exemplu de folosire a cuvântului ''skibidi'': ''What in the skibidi is happening here?'' (Ce naiba/ce nebunie se întâmplă aici?)"
    }
  ]
}'),
	(2, 'Yoiiiii', 2, 1, 'beginner', '{
  "numeLectie": "Introducere in Saluturi",
  "dificultate": "Usor",
  "cuvinteInvatate": [
    { "word": "hello", "translation": "salut" },
    { "word": "goodbye", "translation": "la revedere" },
    { "word": "please", "translation": "te rog" },
    { "word": "thank you", "translation": "mulțumesc" }
  ],
  "teste": [
    {
      "tip": 1,
      "propozitie": "Salut, cum te simti azi?",
      "raspunsCorect": "Hello, how do you feel today?"
    },
    {
      "tip": 2,
      "propozitie": "Mancarea este delicioasa.",
      "raspunsCorect": "The food is delicious."
    },
    {
      "tip": 3,
      "leftWords": ["apple", "house", "car"],
      "rightWords": ["măr", "casă", "mașină"]
    },
    {
      "tip": 4,
      "propozitie": "Definiția cuvântului ''a învăța'' este procesul de dobândire de noi cunoștințe sau abilități."
    }
  ]
}'),
	(5, 'TESTCuvinte', 6, 1, 'advanced', '{
  "numeLectie": "Introducere in Saluturi",
  "dificultate": "Usor",
  "cuvinteInvatate": [
    { "word": "hello", "translation": "salut" },
    { "word": "goodbye", "translation": "la revedere" },
    { "word": "please", "translation": "te rog" },
    { "word": "thank you", "translation": "mulțumesc" }
  ],
  "teste": [
    {
      "tip": 4,
      "propozitie": "Definiția cuvântului ''a învăța'' este procesul de dobândire de noi cunoștințe sau abilități."
    }
  ]
}'),
	(6, 'Alt test (very fast)', 10, 1, 'beginner', '{
  "numeLectie": "Introducere in Saluturi",
  "dificultate": "Usor",
  "cuvinteInvatate": [
    { "word": "to learn", "translation": "a invata" },
    { "word": "to exist", "translation": "a exista" },
    { "word": "please", "translation": "te rog" },
    { "word": "home", "translation": "acasa" }
  ],
  "teste": [
    {
      "tip": 4,
      "propozitie": "Definiția cuvântului ''a învăța'' este procesul de dobândire de noi cunoștințe sau abilități."
    }
  ]
}'),
	(7, 'Alt test (showcase)', 1, 1, 'intermediate', '{
  "numeLectie": "Introducere in Saluturi",
  "dificultate": "Usor",
  "cuvinteInvatate": [
    { "word": "adequate", "translation": "adecvat" },
    { "word": "goodbye", "translation": "la revedere" },
    { "word": "please", "translation": "te rog" },
    { "word": "thank you", "translation": "mulțumesc" }
  ],
  "teste": [
    {
      "tip": 4,
      "propozitie": "Definiția cuvântului ''a învăța'' este procesul de dobândire de noi cunoștințe sau abilități."
    }
  ]
}');


--
-- Data for Name: VocabularyWords; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."VocabularyWords" VALUES
	(68, 'hello', 'salut', NULL, 5),
	(69, 'goodbye', 'la revedere', NULL, 5),
	(70, 'please', 'te rog', NULL, 5),
	(71, 'thank you', 'mulțumesc', NULL, 5),
	(72, 'to learn', 'a invata', NULL, 6),
	(73, 'to exist', 'a exista', NULL, 6),
	(74, 'please', 'te rog', NULL, 6),
	(75, 'home', 'acasa', NULL, 6),
	(76, 'adequate', 'adecvat', NULL, 7),
	(77, 'goodbye', 'la revedere', NULL, 7),
	(78, 'please', 'te rog', NULL, 7),
	(79, 'thank you', 'mulțumesc', NULL, 7);


--
-- Name: Achievements_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Achievements_Id_seq"', 6, true);


--
-- Name: Chapters_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Chapters_Id_seq"', 4, true);


--
-- Name: Courses_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Courses_Id_seq"', 5, true);


--
-- Name: Lessons_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Lessons_Id_seq"', 7, true);


--
-- Name: VocabularyWords_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."VocabularyWords_Id_seq"', 79, true);


--
-- PostgreSQL database dump complete
--


SET session_replication_role = origin;
