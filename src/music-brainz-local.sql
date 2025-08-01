--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2025-07-02 13:12:12

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
-- TOC entry 236 (class 1259 OID 17774)
-- Name: MusicBrainzArtistReleaseMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."MusicBrainzArtistReleaseMap" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MusicBrainzArtistReleaseMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 228 (class 1259 OID 17609)
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
-- TOC entry 229 (class 1259 OID 17616)
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
-- TOC entry 230 (class 1259 OID 17623)
-- Name: MusicBrainzRelease; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzRelease" (
    "Id" uuid NOT NULL,
    "Title" character varying,
    "Disambiguation" character varying,
    "Asin" character varying,
    "Barcode" character varying,
    "Country" character varying,
    "Date" timestamp without time zone,
    "Packaging" character varying,
    "Quality" character varying,
    "Status" character varying,
    "Annotation" character varying
);


ALTER TABLE public."MusicBrainzRelease" OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 17653)
-- Name: MusicBrainzTrack; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzTrack" (
    "Id" uuid NOT NULL,
    "MusicBrainzMediumId" uuid,
    "MusicBrainzRecordingId" uuid,
    "Title" character varying,
    "Position" integer,
    "Length" interval,
    "Number" character varying
);


ALTER TABLE public."MusicBrainzTrack" OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 17775)
-- Name: MusicBrainzCombinedRecordView; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public."MusicBrainzCombinedRecordView" AS
 SELECT track."Id" AS "TrackId",
    medium."Id" AS "MediumId",
    recording."Id" AS "RecordingId",
    release."Id" AS "ReleaseId",
    artist."Id" AS "ArtistId",
    release."Title" AS "Album",
    artist."Name" AS "Artist",
    artist."SortName" AS "ArtistSort",
    recording."Title" AS "RecordingTitle",
    track."Title" AS "TrackTitle",
    track."Position" AS "TrackPosition",
    track."Length" AS "TrackDuration",
    medium."TrackCount",
    medium."TrackOffset",
    release."Asin" AS "AmazonId",
    release."Barcode",
    release."Country",
    release."Date",
    release."Packaging",
    release."Quality",
    release."Status",
    release."Annotation" AS "Comment"
   FROM (((((public."MusicBrainzTrack" track
     JOIN public."MusicBrainzRecording" recording ON ((track."MusicBrainzRecordingId" = recording."Id")))
     JOIN public."MusicBrainzMedium" medium ON ((track."MusicBrainzMediumId" = medium."Id")))
     JOIN public."MusicBrainzRelease" release ON ((medium."MusicBrainzReleaseId" = release."Id")))
     JOIN public."MusicBrainzArtistReleaseMap" map ON ((medium."MusicBrainzReleaseId" = map."MusicBrainzReleaseId")))
     JOIN public."MusicBrainzArtist" artist ON ((map."MusicBrainzArtistId" = artist."Id")));


ALTER VIEW public."MusicBrainzCombinedRecordView" OWNER TO postgres;

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
-- TOC entry 223 (class 1259 OID 17584)
-- Name: MusicBrainzGenre; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzGenre" (
    "Id" uuid NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."MusicBrainzGenre" OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 17591)
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
-- TOC entry 238 (class 1259 OID 17780)
-- Name: MusicBrainzGenreEntityMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."MusicBrainzGenreEntityMap" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MusicBrainzGenreEntityMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 225 (class 1259 OID 17596)
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
-- TOC entry 227 (class 1259 OID 17604)
-- Name: MusicBrainzLabelReleaseMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzLabelReleaseMap" (
    "Id" integer NOT NULL,
    "MusicBrainzLabelId" uuid NOT NULL,
    "MusicBrainzReleaseId" uuid NOT NULL
);


ALTER TABLE public."MusicBrainzLabelReleaseMap" OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 17603)
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
-- TOC entry 231 (class 1259 OID 17635)
-- Name: MusicBrainzTag; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzTag" (
    "Id" uuid NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."MusicBrainzTag" OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 17642)
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
-- TOC entry 239 (class 1259 OID 17781)
-- Name: MusicBrainzTagEntityMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."MusicBrainzTagEntityMap" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MusicBrainzTagEntityMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 234 (class 1259 OID 17660)
-- Name: MusicBrainzUrl; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MusicBrainzUrl" (
    "Id" uuid NOT NULL,
    "Url" character varying NOT NULL
);


ALTER TABLE public."MusicBrainzUrl" OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 17667)
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
-- TOC entry 240 (class 1259 OID 17782)
-- Name: MusicBrainzUrlEntityMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."MusicBrainzUrlEntityMap" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MusicBrainzUrlEntityMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 4817 (class 2606 OID 17568)
-- Name: MusicBrainzArtistRecordingMap MusicBrainzArtistRecordingMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistRecordingMap"
    ADD CONSTRAINT "MusicBrainzArtistRecordingMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4819 (class 2606 OID 17573)
-- Name: MusicBrainzArtistReleaseMap MusicBrainzArtistReleaseMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistReleaseMap"
    ADD CONSTRAINT "MusicBrainzArtistReleaseMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4815 (class 2606 OID 17562)
-- Name: MusicBrainzArtist MusicBrainzArtist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtist"
    ADD CONSTRAINT "MusicBrainzArtist_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4813 (class 2606 OID 17555)
-- Name: MusicBrainzEntityType MusicBrainzEntityType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzEntityType"
    ADD CONSTRAINT "MusicBrainzEntityType_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4823 (class 2606 OID 17595)
-- Name: MusicBrainzGenreEntityMap MusicBrainzGenreEntityMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzGenreEntityMap"
    ADD CONSTRAINT "MusicBrainzGenreEntityMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4821 (class 2606 OID 17590)
-- Name: MusicBrainzGenre MusicBrainzGenre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzGenre"
    ADD CONSTRAINT "MusicBrainzGenre_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4827 (class 2606 OID 17608)
-- Name: MusicBrainzLabelReleaseMap MusicBrainzLabelReleaseMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzLabelReleaseMap"
    ADD CONSTRAINT "MusicBrainzLabelReleaseMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4825 (class 2606 OID 17602)
-- Name: MusicBrainzLabel MusicBrainzLabel_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzLabel"
    ADD CONSTRAINT "MusicBrainzLabel_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4829 (class 2606 OID 17615)
-- Name: MusicBrainzMedium MusicBrainzMedium_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzMedium"
    ADD CONSTRAINT "MusicBrainzMedium_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4831 (class 2606 OID 17622)
-- Name: MusicBrainzRecording MusicBrainzRecording_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzRecording"
    ADD CONSTRAINT "MusicBrainzRecording_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4833 (class 2606 OID 17629)
-- Name: MusicBrainzRelease MusicBrainzRelease_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzRelease"
    ADD CONSTRAINT "MusicBrainzRelease_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4837 (class 2606 OID 17646)
-- Name: MusicBrainzTagEntityMap MusicBrainzTagEntityMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTagEntityMap"
    ADD CONSTRAINT "MusicBrainzTagEntityMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4835 (class 2606 OID 17641)
-- Name: MusicBrainzTag MusicBrainzTag_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTag"
    ADD CONSTRAINT "MusicBrainzTag_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4839 (class 2606 OID 17659)
-- Name: MusicBrainzTrack MusicBrainzTrack_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTrack"
    ADD CONSTRAINT "MusicBrainzTrack_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4843 (class 2606 OID 17671)
-- Name: MusicBrainzUrlEntityMap MusicBrainzUrlEntityMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzUrlEntityMap"
    ADD CONSTRAINT "MusicBrainzUrlEntityMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4841 (class 2606 OID 17666)
-- Name: MusicBrainzUrl MusicBrainzUrl_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzUrl"
    ADD CONSTRAINT "MusicBrainzUrl_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4844 (class 2606 OID 17673)
-- Name: MusicBrainzArtistRecordingMap MusicBrainzArtistEntity_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistRecordingMap"
    ADD CONSTRAINT "MusicBrainzArtistEntity_ForeignKey" FOREIGN KEY ("MusicBrainzArtistId") REFERENCES public."MusicBrainzArtist"("Id") NOT VALID;


--
-- TOC entry 4846 (class 2606 OID 17683)
-- Name: MusicBrainzArtistReleaseMap MusicBrainzArtist_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistReleaseMap"
    ADD CONSTRAINT "MusicBrainzArtist_ForeignKey" FOREIGN KEY ("MusicBrainzArtistId") REFERENCES public."MusicBrainzArtist"("Id") NOT VALID;


--
-- TOC entry 4848 (class 2606 OID 17718)
-- Name: MusicBrainzGenreEntityMap MusicBrainzEntityType_ForegnKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzGenreEntityMap"
    ADD CONSTRAINT "MusicBrainzEntityType_ForegnKey" FOREIGN KEY ("MusicBrainzEntityTypeId") REFERENCES public."MusicBrainzEntityType"("Id") NOT VALID;


--
-- TOC entry 4853 (class 2606 OID 17743)
-- Name: MusicBrainzTagEntityMap MusicBrainzEntityType_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTagEntityMap"
    ADD CONSTRAINT "MusicBrainzEntityType_ForeignKey" FOREIGN KEY ("MusicBrainzEntityTypeId") REFERENCES public."MusicBrainzEntityType"("Id") NOT VALID;


--
-- TOC entry 4857 (class 2606 OID 17763)
-- Name: MusicBrainzUrlEntityMap MusicBrainzEntityType_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzUrlEntityMap"
    ADD CONSTRAINT "MusicBrainzEntityType_ForeignKey" FOREIGN KEY ("MusicBrainzEntityTypeId") REFERENCES public."MusicBrainzEntityType"("Id") NOT VALID;


--
-- TOC entry 4849 (class 2606 OID 17713)
-- Name: MusicBrainzGenreEntityMap MusicBrainzGenre_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzGenreEntityMap"
    ADD CONSTRAINT "MusicBrainzGenre_ForeignKey" FOREIGN KEY ("MusicBrainzGenreId") REFERENCES public."MusicBrainzGenre"("Id") NOT VALID;


--
-- TOC entry 4850 (class 2606 OID 17723)
-- Name: MusicBrainzLabelReleaseMap MusicBrainzLabel_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzLabelReleaseMap"
    ADD CONSTRAINT "MusicBrainzLabel_ForeignKey" FOREIGN KEY ("MusicBrainzLabelId") REFERENCES public."MusicBrainzLabel"("Id") NOT VALID;


--
-- TOC entry 4855 (class 2606 OID 17748)
-- Name: MusicBrainzTrack MusicBrainzMedium_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTrack"
    ADD CONSTRAINT "MusicBrainzMedium_ForeignKey" FOREIGN KEY ("MusicBrainzMediumId") REFERENCES public."MusicBrainzMedium"("Id") NOT VALID;


--
-- TOC entry 4845 (class 2606 OID 17678)
-- Name: MusicBrainzArtistRecordingMap MusicBrainzRecording_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistRecordingMap"
    ADD CONSTRAINT "MusicBrainzRecording_ForeignKey" FOREIGN KEY ("MusicBrainzRecordingId") REFERENCES public."MusicBrainzRecording"("Id") NOT VALID;


--
-- TOC entry 4856 (class 2606 OID 17753)
-- Name: MusicBrainzTrack MusicBrainzRecording_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTrack"
    ADD CONSTRAINT "MusicBrainzRecording_ForeignKey" FOREIGN KEY ("MusicBrainzRecordingId") REFERENCES public."MusicBrainzRecording"("Id") NOT VALID;


--
-- TOC entry 4847 (class 2606 OID 17688)
-- Name: MusicBrainzArtistReleaseMap MusicBrainzRelease_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzArtistReleaseMap"
    ADD CONSTRAINT "MusicBrainzRelease_ForeignKey" FOREIGN KEY ("MusicBrainzReleaseId") REFERENCES public."MusicBrainzRelease"("Id") NOT VALID;


--
-- TOC entry 4851 (class 2606 OID 17728)
-- Name: MusicBrainzLabelReleaseMap MusicBrainzRelease_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzLabelReleaseMap"
    ADD CONSTRAINT "MusicBrainzRelease_ForeignKey" FOREIGN KEY ("MusicBrainzReleaseId") REFERENCES public."MusicBrainzRelease"("Id") NOT VALID;


--
-- TOC entry 4852 (class 2606 OID 17733)
-- Name: MusicBrainzMedium MusicBrainzRelease_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzMedium"
    ADD CONSTRAINT "MusicBrainzRelease_ForeignKey" FOREIGN KEY ("MusicBrainzReleaseId") REFERENCES public."MusicBrainzRelease"("Id") NOT VALID;


--
-- TOC entry 4854 (class 2606 OID 17738)
-- Name: MusicBrainzTagEntityMap MusicBrainzTag_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzTagEntityMap"
    ADD CONSTRAINT "MusicBrainzTag_ForeignKey" FOREIGN KEY ("MusicBrainzTagId") REFERENCES public."MusicBrainzTag"("Id") NOT VALID;


--
-- TOC entry 4858 (class 2606 OID 17758)
-- Name: MusicBrainzUrlEntityMap MusicBrainzUrl_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MusicBrainzUrlEntityMap"
    ADD CONSTRAINT "MusicBrainzUrl_ForeignKey" FOREIGN KEY ("MusicBrainzUrlId") REFERENCES public."MusicBrainzUrl"("Id") NOT VALID;


-- Completed on 2025-07-02 13:12:13

--
-- PostgreSQL database dump complete
--

