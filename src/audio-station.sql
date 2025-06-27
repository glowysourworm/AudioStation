--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2025-06-27 17:02:20

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
-- Name: M3UStream; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."M3UStream" (
    "Id" integer NOT NULL,
    "Duration" integer NOT NULL,
    "Name" character varying NOT NULL,
    "GroupName" character varying,
    "LogoUrl" character varying,
    "HomepageUrl" character varying,
    "StreamSourceUrl" character varying NOT NULL,
    "UserExcluded" boolean NOT NULL
);


ALTER TABLE public."M3UStream" OWNER TO postgres;

--
-- TOC entry 4950 (class 0 OID 0)
-- Dependencies: 218
-- Name: TABLE "M3UStream"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."M3UStream" IS 'Details of an M3U file. This example is taken from the m3uParser .NET library fields.';


--
-- TOC entry 217 (class 1259 OID 16768)
-- Name: M3UInfo_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."M3UStream" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."M3UInfo_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 221 (class 1259 OID 16823)
-- Name: Mp3FileReference; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReference" (
    "Id" integer NOT NULL,
    "FileName" character varying NOT NULL,
    "Title" character varying,
    "Track" integer,
    "AlbumId" integer,
    "PrimaryArtistId" integer,
    "DurationMilliseconds" integer,
    "PrimaryGenreId" integer,
    "IsFileAvailable" boolean NOT NULL,
    "IsFileCorrupt" boolean NOT NULL,
    "IsFileLoadError" boolean NOT NULL,
    "FileErrorMessage" character varying,
    "FileCorruptMessage" character varying,
    "AmazonId" character varying,
    "MusicBrainzTrackId" character varying
);


ALTER TABLE public."Mp3FileReference" OWNER TO postgres;

--
-- TOC entry 4951 (class 0 OID 0)
-- Dependencies: 221
-- Name: TABLE "Mp3FileReference"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."Mp3FileReference" IS 'Portion of an mp3 file''s data used to quickly load the library on startup. Mp3 files are also loaded at runtime to verify tag data and use / modify tags. Artwork is also loaded at runtime.';


--
-- TOC entry 225 (class 1259 OID 16844)
-- Name: Mp3FileReferenceAlbum; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceAlbum" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL,
    "DiscNumber" integer,
    "DiscCount" integer,
    "Year" integer,
    "MusicBrainzReleaseId" character varying
);


ALTER TABLE public."Mp3FileReferenceAlbum" OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 16843)
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
-- TOC entry 223 (class 1259 OID 16831)
-- Name: Mp3FileReferenceArtist; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceArtist" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL,
    "MusicBrainzArtistId" character varying
);


ALTER TABLE public."Mp3FileReferenceArtist" OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 16875)
-- Name: Mp3FileReferenceArtistMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceArtistMap" (
    "Id" integer NOT NULL,
    "Mp3FileReferenceId" integer NOT NULL,
    "Mp3FileReferenceArtistId" integer NOT NULL,
    "IsPrimaryArtist" boolean NOT NULL
);


ALTER TABLE public."Mp3FileReferenceArtistMap" OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 16874)
-- Name: Mp3FileReferenceArtistMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Mp3FileReferenceArtistMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceArtistMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 222 (class 1259 OID 16830)
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
-- TOC entry 227 (class 1259 OID 16857)
-- Name: Mp3FileReferenceGenre; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceGenre" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."Mp3FileReferenceGenre" OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 16896)
-- Name: Mp3FileReferenceGenreMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Mp3FileReferenceGenreMap" (
    "Id" integer NOT NULL,
    "Mp3FileReferenceId" integer NOT NULL,
    "Mp3FileReferenceGenreId" integer NOT NULL,
    "IsPrimaryGenre" boolean NOT NULL
);


ALTER TABLE public."Mp3FileReferenceGenreMap" OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 16895)
-- Name: Mp3FileReferenceGenreMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Mp3FileReferenceGenreMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceGenreMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 226 (class 1259 OID 16856)
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
-- TOC entry 220 (class 1259 OID 16822)
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
-- TOC entry 219 (class 1259 OID 16815)
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
-- TOC entry 4777 (class 2606 OID 16775)
-- Name: M3UStream M3UInfo_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UStream"
    ADD CONSTRAINT "M3UInfo_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4786 (class 2606 OID 16850)
-- Name: Mp3FileReferenceAlbum Mp3FileReferenceAlbum_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceAlbum"
    ADD CONSTRAINT "Mp3FileReferenceAlbum_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4790 (class 2606 OID 16879)
-- Name: Mp3FileReferenceArtistMap Mp3FileReferenceArtistMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceArtistMap"
    ADD CONSTRAINT "Mp3FileReferenceArtistMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4784 (class 2606 OID 16837)
-- Name: Mp3FileReferenceArtist Mp3FileReferenceArtist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceArtist"
    ADD CONSTRAINT "Mp3FileReferenceArtist_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4792 (class 2606 OID 16900)
-- Name: Mp3FileReferenceGenreMap Mp3FileReferenceGenreMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceGenreMap"
    ADD CONSTRAINT "Mp3FileReferenceGenreMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4788 (class 2606 OID 16863)
-- Name: Mp3FileReferenceGenre Mp3FileReferenceGenre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceGenre"
    ADD CONSTRAINT "Mp3FileReferenceGenre_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4782 (class 2606 OID 16829)
-- Name: Mp3FileReference Mp3FileReference_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReference"
    ADD CONSTRAINT "Mp3FileReference_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4780 (class 2606 OID 16821)
-- Name: RadioBrowserStation RadioBrowserStation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RadioBrowserStation"
    ADD CONSTRAINT "RadioBrowserStation_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4778 (class 1259 OID 17220)
-- Name: NameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "NameIndex" ON public."M3UStream" USING btree ("Name") WITH (deduplicate_items='true');


--
-- TOC entry 4793 (class 2606 OID 16869)
-- Name: Mp3FileReference Mp3FileReferenceAlbum_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReference"
    ADD CONSTRAINT "Mp3FileReferenceAlbum_ForeignKey" FOREIGN KEY ("AlbumId") REFERENCES public."Mp3FileReferenceAlbum"("Id");


--
-- TOC entry 4796 (class 2606 OID 16885)
-- Name: Mp3FileReferenceArtistMap Mp3FileReferenceArtist_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceArtistMap"
    ADD CONSTRAINT "Mp3FileReferenceArtist_ForeignKey" FOREIGN KEY ("Mp3FileReferenceArtistId") REFERENCES public."Mp3FileReferenceArtist"("Id");


--
-- TOC entry 4794 (class 2606 OID 16890)
-- Name: Mp3FileReference Mp3FileReferenceArtist_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReference"
    ADD CONSTRAINT "Mp3FileReferenceArtist_ForeignKey" FOREIGN KEY ("PrimaryArtistId") REFERENCES public."Mp3FileReferenceArtist"("Id") NOT VALID;


--
-- TOC entry 4798 (class 2606 OID 16906)
-- Name: Mp3FileReferenceGenreMap Mp3FileReferenceGenre_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceGenreMap"
    ADD CONSTRAINT "Mp3FileReferenceGenre_ForeignKey" FOREIGN KEY ("Mp3FileReferenceGenreId") REFERENCES public."Mp3FileReferenceGenre"("Id");


--
-- TOC entry 4795 (class 2606 OID 17237)
-- Name: Mp3FileReference Mp3FileReferenceGenre_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReference"
    ADD CONSTRAINT "Mp3FileReferenceGenre_ForeignKey" FOREIGN KEY ("PrimaryGenreId") REFERENCES public."Mp3FileReferenceGenre"("Id") NOT VALID;


--
-- TOC entry 4797 (class 2606 OID 16880)
-- Name: Mp3FileReferenceArtistMap Mp3FileReference_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceArtistMap"
    ADD CONSTRAINT "Mp3FileReference_ForeignKey" FOREIGN KEY ("Mp3FileReferenceId") REFERENCES public."Mp3FileReference"("Id");


--
-- TOC entry 4799 (class 2606 OID 16901)
-- Name: Mp3FileReferenceGenreMap Mp3FileReference_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Mp3FileReferenceGenreMap"
    ADD CONSTRAINT "Mp3FileReference_ForeignKey" FOREIGN KEY ("Mp3FileReferenceId") REFERENCES public."Mp3FileReference"("Id");


-- Completed on 2025-06-27 17:02:20

--
-- PostgreSQL database dump complete
--

