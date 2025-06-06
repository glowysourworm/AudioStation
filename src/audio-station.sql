--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2025-06-05 23:30:42

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
-- TOC entry 218 (class 1259 OID 16769)
-- Name: M3UInfo; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."M3UInfo" (
    "Id" integer NOT NULL,
    "PlaylistType" character varying NOT NULL,
    "TargetDurationMilliseconds" integer,
    "Version" integer,
    "MediaSequence" integer,
    "UserExcluded" bit(1) NOT NULL
);


ALTER TABLE public."M3UInfo" OWNER TO postgres;

--
-- TOC entry 4955 (class 0 OID 0)
-- Dependencies: 218
-- Name: TABLE "M3UInfo"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."M3UInfo" IS 'Details of an M3U file. This example is taken from the m3uParser .NET library fields.';


--
-- TOC entry 220 (class 1259 OID 16777)
-- Name: M3UInfoAttributes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."M3UInfoAttributes" (
    "Id" integer NOT NULL,
    "M3UInfoId" integer,
    "GroupTitle" character varying NOT NULL,
    "TvgShift" character varying NOT NULL,
    "TvgName" character varying NOT NULL,
    "TvgLogo" character varying NOT NULL,
    "AudioTrack" character varying NOT NULL,
    "AspectRatio" character varying NOT NULL,
    "TvgId" character varying NOT NULL,
    "UrlTvg" character varying NOT NULL,
    "M3UAutoLoad" integer,
    "Cache" integer,
    "Deinterlace" integer,
    "Refresh" integer,
    "ChannelNumber" integer,
    "M3UMediaId" integer
);


ALTER TABLE public."M3UInfoAttributes" OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16776)
-- Name: M3UInfoAttributes_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."M3UInfoAttributes" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."M3UInfoAttributes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 224 (class 1259 OID 16803)
-- Name: M3UInfoWarning; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."M3UInfoWarning" (
    "Id" integer NOT NULL,
    "M3UInfoId" integer NOT NULL,
    "Warning" character varying NOT NULL
);


ALTER TABLE public."M3UInfoWarning" OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 16802)
-- Name: M3UInfoWarning_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."M3UInfoWarning" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."M3UInfoWarning_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 217 (class 1259 OID 16768)
-- Name: M3UInfo_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."M3UInfo" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."M3UInfo_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 222 (class 1259 OID 16785)
-- Name: M3UMedia; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."M3UMedia" (
    "Id" integer NOT NULL,
    "DurationMilliseconds" integer NOT NULL,
    "RawTitle" character varying NOT NULL,
    "InnerTitle" character varying NOT NULL,
    "MediaFile" character varying NOT NULL,
    "UserExcluded" bit(1) NOT NULL
);


ALTER TABLE public."M3UMedia" OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16784)
-- Name: M3UMedia_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."M3UMedia" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."M3UMedia_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 227 (class 1259 OID 16823)
-- Name: Mp3FileReference; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReference" (
    "Id" integer NOT NULL,
    "FileName" character varying NOT NULL,
    "Title" character varying NOT NULL,
    "Album" character varying NOT NULL,
    "PrimaryArtist" character varying NOT NULL,
    "Track" integer NOT NULL
);


ALTER TABLE public."Mp3FileReference" OWNER TO postgres;

--
-- TOC entry 4956 (class 0 OID 0)
-- Dependencies: 227
-- Name: TABLE "Mp3FileReference"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."Mp3FileReference" IS 'Portion of an mp3 file''s data used to quickly load the library on startup. Mp3 files are also loaded at runtime to verify tag data and use / modify tags. Artwork is also loaded at runtime.';


--
-- TOC entry 231 (class 1259 OID 16844)
-- Name: Mp3FileReferenceAlbum; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceAlbum" (
    "Id" integer NOT NULL,
    "Mp3FileReferenceId" integer NOT NULL,
    "Name" character varying NOT NULL,
    "DiscNumber" integer NOT NULL,
    "DiscCount" integer NOT NULL
);


ALTER TABLE public."Mp3FileReferenceAlbum" OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 16843)
-- Name: Mp3FileReferenceAlbum_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Mp3FileReferenceAlbum" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceAlbum_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 229 (class 1259 OID 16831)
-- Name: Mp3FileReferenceArtist; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceArtist" (
    "Id" integer NOT NULL,
    "Mp3FileReferenceId" integer NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."Mp3FileReferenceArtist" OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 16830)
-- Name: Mp3FileReferenceArtist_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Mp3FileReferenceArtist" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceArtist_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 233 (class 1259 OID 16857)
-- Name: Mp3FileReferenceGenre; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceGenre" (
    "Id" integer NOT NULL,
    "Mp3FileReferenceId" integer NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."Mp3FileReferenceGenre" OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 16856)
-- Name: Mp3FileReferenceGenre_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Mp3FileReferenceGenre" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceGenre_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 226 (class 1259 OID 16822)
-- Name: Mp3FileReference_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Mp3FileReference" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReference_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 225 (class 1259 OID 16815)
-- Name: RadioBrowserStation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."RadioBrowserStation" (
    "Id" integer NOT NULL,
    "StationUUID" uuid NOT NULL,
    "ServerUUID" uuid NOT NULL,
    "Name" character varying NOT NULL,
    "Url" character varying NOT NULL,
    "UrlResolved" character varying NOT NULL,
    "Homepage" character varying NOT NULL,
    "Favicon" character varying NOT NULL,
    "Tags" character varying NOT NULL,
    "Country" character varying NOT NULL,
    "State" character varying NOT NULL,
    "Language" character varying NOT NULL,
    "LanguageCodes" character varying NOT NULL,
    "Codec" character varying NOT NULL,
    "Bitrate" integer NOT NULL,
    "Hls" integer NOT NULL,
    "UserExcluded" bit(1) NOT NULL
);


ALTER TABLE public."RadioBrowserStation" OWNER TO postgres;

--
-- TOC entry 4784 (class 2606 OID 16783)
-- Name: M3UInfoAttributes M3UInfoAttributes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UInfoAttributes"
    ADD CONSTRAINT "M3UInfoAttributes_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4788 (class 2606 OID 16809)
-- Name: M3UInfoWarning M3UInfoWarning_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UInfoWarning"
    ADD CONSTRAINT "M3UInfoWarning_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4782 (class 2606 OID 16775)
-- Name: M3UInfo M3UInfo_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UInfo"
    ADD CONSTRAINT "M3UInfo_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4786 (class 2606 OID 16791)
-- Name: M3UMedia M3UMedia_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UMedia"
    ADD CONSTRAINT "M3UMedia_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4796 (class 2606 OID 16850)
-- Name: Mp3FileReferenceAlbum Mp3FileReferenceAlbum_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceAlbum"
    ADD CONSTRAINT "Mp3FileReferenceAlbum_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4794 (class 2606 OID 16837)
-- Name: Mp3FileReferenceArtist Mp3FileReferenceArtist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceArtist"
    ADD CONSTRAINT "Mp3FileReferenceArtist_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4798 (class 2606 OID 16863)
-- Name: Mp3FileReferenceGenre Mp3FileReferenceGenre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceGenre"
    ADD CONSTRAINT "Mp3FileReferenceGenre_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4792 (class 2606 OID 16829)
-- Name: Mp3FileReference Mp3FileReference_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReference"
    ADD CONSTRAINT "Mp3FileReference_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4790 (class 2606 OID 16821)
-- Name: RadioBrowserStation RadioBrowserStation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RadioBrowserStation"
    ADD CONSTRAINT "RadioBrowserStation_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4799 (class 2606 OID 16797)
-- Name: M3UInfoAttributes M3UInfo_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UInfoAttributes"
    ADD CONSTRAINT "M3UInfo_ForeignKey" FOREIGN KEY ("M3UInfoId") REFERENCES public."M3UInfo"("Id");


--
-- TOC entry 4801 (class 2606 OID 16810)
-- Name: M3UInfoWarning M3UInfo_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UInfoWarning"
    ADD CONSTRAINT "M3UInfo_ForeignKey" FOREIGN KEY ("M3UInfoId") REFERENCES public."M3UInfo"("Id");


--
-- TOC entry 4800 (class 2606 OID 16792)
-- Name: M3UInfoAttributes M3UMedia_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UInfoAttributes"
    ADD CONSTRAINT "M3UMedia_ForeignKey" FOREIGN KEY ("M3UMediaId") REFERENCES public."M3UMedia"("Id");


--
-- TOC entry 4802 (class 2606 OID 16838)
-- Name: Mp3FileReferenceArtist Mp3FileReference_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceArtist"
    ADD CONSTRAINT "Mp3FileReference_ForeignKey" FOREIGN KEY ("Mp3FileReferenceId") REFERENCES public."Mp3FileReference"("Id");


--
-- TOC entry 4803 (class 2606 OID 16851)
-- Name: Mp3FileReferenceAlbum Mp3FileReference_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceAlbum"
    ADD CONSTRAINT "Mp3FileReference_ForeignKey" FOREIGN KEY ("Mp3FileReferenceId") REFERENCES public."Mp3FileReference"("Id");


--
-- TOC entry 4804 (class 2606 OID 16864)
-- Name: Mp3FileReferenceGenre Mp3FileReference_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceGenre"
    ADD CONSTRAINT "Mp3FileReference_ForeignKey" FOREIGN KEY ("Mp3FileReferenceId") REFERENCES public."Mp3FileReference"("Id");


-- Completed on 2025-06-05 23:30:42

--
-- PostgreSQL database dump complete
--

