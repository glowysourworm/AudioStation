--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2025-06-27 17:40:29

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

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 219 (class 1259 OID 17556)
-- Name: MusicBrainzArtist; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzArtist" (
    "Id" uuid NOT NULL,
    "Name" character varying,
    "SortName" character varying,
    "Disambiguation" character varying,
    "Country" character varying,
    "Annotation" character varying
);


ALTER TABLE public."MusicBrainzArtist" OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 17564)
-- Name: MusicBrainzArtistRecordingMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzArtistRecordingMap" (
    "Id" integer NOT NULL,
    "MusicBrainzArtistId" uuid NOT NULL,
    "MusicBrainzRecordingId" uuid NOT NULL
);


ALTER TABLE public."MusicBrainzArtistRecordingMap" OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 17563)
-- Name: MusicBrainzArtistRecordingMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."MusicBrainzArtistRecordingMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."MusicBrainzArtistRecordingMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 222 (class 1259 OID 17569)
-- Name: MusicBrainzArtistReleaseMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzArtistReleaseMap" (
    "Id" integer NOT NULL,
    "MusicBrainzArtistId" uuid NOT NULL,
    "MusicBrainzReleaseId" uuid NOT NULL
);


ALTER TABLE public."MusicBrainzArtistReleaseMap" OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 17574)
-- Name: MusicBrainzArtistTrackMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzArtistTrackMap" (
    "Id" integer NOT NULL,
    "MusicBrainzArtistId" uuid NOT NULL,
    "MusicBrainzTrackId" uuid NOT NULL
);


ALTER TABLE public."MusicBrainzArtistTrackMap" OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 17579)
-- Name: MusicBrainzDisc; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzDisc" (
    "Id" uuid NOT NULL,
    "MusicBrainzReleaseId" uuid NOT NULL,
    "MusicBrainzMediumId" uuid NOT NULL
);


ALTER TABLE public."MusicBrainzDisc" OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 17549)
-- Name: MusicBrainzEntityType; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzEntityType" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."MusicBrainzEntityType" OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 17548)
-- Name: MusicBrainzEntityType_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."MusicBrainzEntityType" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."MusicBrainzEntityType_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 225 (class 1259 OID 17584)
-- Name: MusicBrainzGenre; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzGenre" (
    "Id" uuid NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."MusicBrainzGenre" OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 17591)
-- Name: MusicBrainzGenreEntityMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzGenreEntityMap" (
    "Id" integer NOT NULL,
    "MusicBrainzGenreId" uuid NOT NULL,
    "MusicBrainzEntityId" uuid NOT NULL,
    "MusicBrainzEntityTypeId" integer NOT NULL
);


ALTER TABLE public."MusicBrainzGenreEntityMap" OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 17596)
-- Name: MusicBrainzLabel; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzLabel" (
    "Id" uuid NOT NULL,
    "LabelCode" integer,
    "Name" character varying,
    "SortName" character varying,
    "Disambiguation" character varying,
    "Type" character varying,
    "Country" character varying,
    "Annotation" character varying
);


ALTER TABLE public."MusicBrainzLabel" OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 17604)
-- Name: MusicBrainzLabelReleaseMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzLabelReleaseMap" (
    "Id" integer NOT NULL,
    "MusicBrainzLabelId" uuid NOT NULL,
    "MusicBrainzReleaseId" uuid NOT NULL
);


ALTER TABLE public."MusicBrainzLabelReleaseMap" OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 17603)
-- Name: MusicBrainzLabelReleaseMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."MusicBrainzLabelReleaseMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."MusicBrainzLabelReleaseMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 230 (class 1259 OID 17609)
-- Name: MusicBrainzMedium; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzMedium" (
    "Id" uuid NOT NULL,
    "MusicBrainzReleaseId" uuid NOT NULL,
    "Title" character varying,
    "Format" character varying,
    "Position" integer NOT NULL,
    "TrackCount" integer NOT NULL,
    "TrackOffset" integer
);


ALTER TABLE public."MusicBrainzMedium" OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 17616)
-- Name: MusicBrainzRecording; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzRecording" (
    "Id" uuid NOT NULL,
    "Title" character varying,
    "Disambiguation" character varying,
    "Length" interval,
    "Annotation" character varying
);


ALTER TABLE public."MusicBrainzRecording" OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 17623)
-- Name: MusicBrainzRelease; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzRelease" (
    "Id" uuid NOT NULL,
    "Title" character varying,
    "Disambiguation" character varying,
    "Asin" character varying,
    "Barcode" character varying,
    "Country" character varying,
    "Date" timestamp with time zone,
    "Packaging" character varying,
    "Quality" character varying,
    "Status" character varying,
    "Annotation" character varying
);


ALTER TABLE public."MusicBrainzRelease" OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 17630)
-- Name: MusicBrainzReleaseLabelMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzReleaseLabelMap" (
    "Id" integer NOT NULL,
    "MusicBrainzReleaseId" uuid NOT NULL,
    "MusicBrainzLabelId" uuid NOT NULL
);


ALTER TABLE public."MusicBrainzReleaseLabelMap" OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 17635)
-- Name: MusicBrainzTag; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzTag" (
    "Id" uuid NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."MusicBrainzTag" OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 17642)
-- Name: MusicBrainzTagEntityMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzTagEntityMap" (
    "Id" integer NOT NULL,
    "MusicBrainzTagId" uuid NOT NULL,
    "MusicBrainzEntityId" uuid NOT NULL,
    "MusicBrainzEntityTypeId" bigint NOT NULL
);


ALTER TABLE public."MusicBrainzTagEntityMap" OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 17653)
-- Name: MusicBrainzTrack; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzTrack" (
    "Id" uuid NOT NULL,
    "MusicBrainzMediumId" uuid NOT NULL,
    "MusicBrainzRecordingId" uuid NOT NULL,
    "Title" character varying,
    "Number" integer,
    "Position" integer,
    "Length" interval
);


ALTER TABLE public."MusicBrainzTrack" OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 17660)
-- Name: MusicBrainzUrl; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzUrl" (
    "Id" uuid NOT NULL,
    "Url" character varying NOT NULL
);


ALTER TABLE public."MusicBrainzUrl" OWNER TO postgres;

--
-- TOC entry 238 (class 1259 OID 17667)
-- Name: MusicBrainzUrlEntityMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzUrlEntityMap" (
    "Id" integer NOT NULL,
    "MusicBrainzUrlId" uuid NOT NULL,
    "MusicBrainzEntityId" uuid NOT NULL,
    "MusicBrainzEntityTypeId" integer NOT NULL
);


ALTER TABLE public."MusicBrainzUrlEntityMap" OWNER TO postgres;

--
-- TOC entry 4821 (class 2606 OID 17568)
-- Name: MusicBrainzArtistRecordingMap MusicBrainzArtistRecordingMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistRecordingMap"
    ADD CONSTRAINT "MusicBrainzArtistRecordingMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4823 (class 2606 OID 17573)
-- Name: MusicBrainzArtistReleaseMap MusicBrainzArtistReleaseMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistReleaseMap"
    ADD CONSTRAINT "MusicBrainzArtistReleaseMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4825 (class 2606 OID 17578)
-- Name: MusicBrainzArtistTrackMap MusicBrainzArtistTrackMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistTrackMap"
    ADD CONSTRAINT "MusicBrainzArtistTrackMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4819 (class 2606 OID 17562)
-- Name: MusicBrainzArtist MusicBrainzArtist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtist"
    ADD CONSTRAINT "MusicBrainzArtist_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4827 (class 2606 OID 17583)
-- Name: MusicBrainzDisc MusicBrainzDisc_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzDisc"
    ADD CONSTRAINT "MusicBrainzDisc_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4817 (class 2606 OID 17555)
-- Name: MusicBrainzEntityType MusicBrainzEntityType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzEntityType"
    ADD CONSTRAINT "MusicBrainzEntityType_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4831 (class 2606 OID 17595)
-- Name: MusicBrainzGenreEntityMap MusicBrainzGenreEntityMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzGenreEntityMap"
    ADD CONSTRAINT "MusicBrainzGenreEntityMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4829 (class 2606 OID 17590)
-- Name: MusicBrainzGenre MusicBrainzGenre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzGenre"
    ADD CONSTRAINT "MusicBrainzGenre_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4835 (class 2606 OID 17608)
-- Name: MusicBrainzLabelReleaseMap MusicBrainzLabelReleaseMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzLabelReleaseMap"
    ADD CONSTRAINT "MusicBrainzLabelReleaseMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4833 (class 2606 OID 17602)
-- Name: MusicBrainzLabel MusicBrainzLabel_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzLabel"
    ADD CONSTRAINT "MusicBrainzLabel_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4837 (class 2606 OID 17615)
-- Name: MusicBrainzMedium MusicBrainzMedium_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzMedium"
    ADD CONSTRAINT "MusicBrainzMedium_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4839 (class 2606 OID 17622)
-- Name: MusicBrainzRecording MusicBrainzRecording_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzRecording"
    ADD CONSTRAINT "MusicBrainzRecording_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4843 (class 2606 OID 17648)
-- Name: MusicBrainzReleaseLabelMap MusicBrainzReleaseLabelMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzReleaseLabelMap"
    ADD CONSTRAINT "MusicBrainzReleaseLabelMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4841 (class 2606 OID 17629)
-- Name: MusicBrainzRelease MusicBrainzRelease_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzRelease"
    ADD CONSTRAINT "MusicBrainzRelease_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4847 (class 2606 OID 17646)
-- Name: MusicBrainzTagEntityMap MusicBrainzTagEntityMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTagEntityMap"
    ADD CONSTRAINT "MusicBrainzTagEntityMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4845 (class 2606 OID 17641)
-- Name: MusicBrainzTag MusicBrainzTag_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTag"
    ADD CONSTRAINT "MusicBrainzTag_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4849 (class 2606 OID 17659)
-- Name: MusicBrainzTrack MusicBrainzTrack_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTrack"
    ADD CONSTRAINT "MusicBrainzTrack_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4853 (class 2606 OID 17671)
-- Name: MusicBrainzUrlEntityMap MusicBrainzUrlEntityMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzUrlEntityMap"
    ADD CONSTRAINT "MusicBrainzUrlEntityMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4851 (class 2606 OID 17666)
-- Name: MusicBrainzUrl MusicBrainzUrl_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzUrl"
    ADD CONSTRAINT "MusicBrainzUrl_pkey" PRIMARY KEY ("Id");


-- Completed on 2025-06-27 17:40:29

--
-- PostgreSQL database dump complete
--

