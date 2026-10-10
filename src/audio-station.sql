--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2026-10-10 10:47:08

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
-- TOC entry 223 (class 1259 OID 50663)
-- Name: AcoustIDLookupResult; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AcoustIDLookupResult" (
    "LookupId" uuid NOT NULL,
    "MusicBrainzRecordingId" uuid NOT NULL,
    "Score" double precision NOT NULL,
    "FileName" character varying NOT NULL,
    "Id" uuid NOT NULL
);


ALTER TABLE public."AcoustIDLookupResult" OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16844)
-- Name: Album; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Album" (
    "Name" character varying NOT NULL,
    "MediaCount" integer NOT NULL,
    "Year" integer NOT NULL,
    "MediaFormat" character varying NOT NULL,
    "TrackCount" integer NOT NULL,
    "Id" uuid NOT NULL,
    "ArtistId" uuid NOT NULL
);


ALTER TABLE public."Album" OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 52170)
-- Name: AlbumFileReferenceMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AlbumFileReferenceMap" (
    "Id" uuid NOT NULL,
    "AlbumId" uuid NOT NULL,
    "FileReferenceId" uuid NOT NULL
);


ALTER TABLE public."AlbumFileReferenceMap" OWNER TO postgres;

--
-- TOC entry 4997 (class 0 OID 0)
-- Dependencies: 228
-- Name: TABLE "AlbumFileReferenceMap"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."AlbumFileReferenceMap" IS 'This table should track any files related to the album art. There will be a column for the type which will separate files based on their purpose for the ablum.';


--
-- TOC entry 220 (class 1259 OID 16831)
-- Name: Artist; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Artist" (
    "Name" character varying NOT NULL,
    "Id" uuid NOT NULL
);


ALTER TABLE public."Artist" OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 52175)
-- Name: ArtistFileReferenceMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ArtistFileReferenceMap" (
    "Id" uuid NOT NULL,
    "ArtistId" uuid NOT NULL,
    "FileReferenceId" uuid NOT NULL
);


ALTER TABLE public."ArtistFileReferenceMap" OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 50706)
-- Name: FileReference; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."FileReference" (
    "FileName" character varying NOT NULL,
    "DateAdded" timestamp with time zone NOT NULL,
    "DateLastModified" timestamp with time zone NOT NULL,
    "Id" uuid NOT NULL,
    "FileTypeId" uuid NOT NULL
);


ALTER TABLE public."FileReference" OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 52180)
-- Name: FileType; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."FileType" (
    "Name" character varying NOT NULL,
    "Id" uuid NOT NULL
);


ALTER TABLE public."FileType" OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 16857)
-- Name: Genre; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Genre" (
    "Name" character varying NOT NULL,
    "Id" uuid NOT NULL
);


ALTER TABLE public."Genre" OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 16769)
-- Name: M3UStream; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."M3UStream" (
    "Duration" integer NOT NULL,
    "Name" character varying NOT NULL,
    "GroupName" character varying,
    "LogoUrl" character varying,
    "HomepageUrl" character varying,
    "StreamSourceUrl" character varying NOT NULL,
    "UserExcluded" boolean NOT NULL,
    "Id" uuid NOT NULL
);


ALTER TABLE public."M3UStream" OWNER TO postgres;

--
-- TOC entry 4998 (class 0 OID 0)
-- Dependencies: 217
-- Name: TABLE "M3UStream"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."M3UStream" IS 'Details of an M3U file. This example is taken from the m3uParser .NET library fields.';


--
-- TOC entry 224 (class 1259 OID 50669)
-- Name: TagSmall; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TagSmall" (
    "AlbumArtist" character varying,
    "Album" character varying,
    "Title" character varying,
    "Genre" character varying,
    "TrackNumber" integer,
    "TrackTotal" integer,
    "MediaNumber" integer,
    "MediaTotal" integer,
    "MediaFormat" character varying,
    "DurationMilliseconds" integer,
    "Year" integer,
    "Id" uuid NOT NULL
);


ALTER TABLE public."TagSmall" OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 52115)
-- Name: TagSmallVendorMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TagSmallVendorMap" (
    "MusicBrainzRecordingId" uuid,
    "Id" uuid NOT NULL,
    "TagSmallId" uuid NOT NULL,
    "VendorId" uuid NOT NULL
);


ALTER TABLE public."TagSmallVendorMap" OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 58284)
-- Name: MusicBrainzAcoustIDResult; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public."MusicBrainzAcoustIDResult" AS
 SELECT tagmap."TagSmallId",
    acoustid."LookupId" AS "AcoustIDLookupId",
    acoustid."MusicBrainzRecordingId",
    acoustid."Score",
    acoustid."FileName",
    tag."Genre",
    tag."AlbumArtist",
    tag."Album",
    tag."Title",
    tag."TrackNumber",
    tag."TrackTotal",
    tag."MediaNumber",
    tag."MediaTotal",
    tag."MediaFormat",
    tag."DurationMilliseconds",
    tag."Year"
   FROM ((public."AcoustIDLookupResult" acoustid
     JOIN public."TagSmallVendorMap" tagmap ON ((acoustid."MusicBrainzRecordingId" = tagmap."MusicBrainzRecordingId")))
     JOIN public."TagSmall" tag ON ((tag."Id" = tagmap."TagSmallId")))
  WHERE (acoustid."MusicBrainzRecordingId" IS NOT NULL);


ALTER VIEW public."MusicBrainzAcoustIDResult" OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 16815)
-- Name: RadioBrowserStation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."RadioBrowserStation" (
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
    "UserExcluded" bit(1) NOT NULL,
    "Id" uuid NOT NULL
);


ALTER TABLE public."RadioBrowserStation" OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 52219)
-- Name: TagSmallFileReferenceMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TagSmallFileReferenceMap" (
    "Id" uuid NOT NULL,
    "TagSmallId" uuid NOT NULL,
    "FileReferenceId" uuid NOT NULL
);


ALTER TABLE public."TagSmallFileReferenceMap" OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16823)
-- Name: Track; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Track" (
    "Title" character varying NOT NULL,
    "TrackNumber" integer NOT NULL,
    "DurationMilliseconds" integer NOT NULL,
    "MediaNumber" integer NOT NULL,
    "Id" uuid NOT NULL,
    "AlbumId" uuid NOT NULL,
    "ArtistId" uuid NOT NULL,
    "GenreId" uuid NOT NULL,
    "FileReferenceId" uuid NOT NULL
);


ALTER TABLE public."Track" OWNER TO postgres;

--
-- TOC entry 4999 (class 0 OID 0)
-- Dependencies: 219
-- Name: TABLE "Track"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."Track" IS 'Portion of an mp3 file''s data used to quickly load the library on startup. Mp3 files are also loaded at runtime to verify tag data and use / modify tags. Artwork is also loaded at runtime.';


--
-- TOC entry 225 (class 1259 OID 50677)
-- Name: Vendor; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Vendor" (
    "Name" character varying NOT NULL,
    "Id" uuid NOT NULL
);


ALTER TABLE public."Vendor" OWNER TO postgres;

--
-- TOC entry 4815 (class 2606 OID 58255)
-- Name: AcoustIDLookupResult AcoustIDLookupResult_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AcoustIDLookupResult"
    ADD CONSTRAINT "AcoustIDLookupResult_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4825 (class 2606 OID 58259)
-- Name: AlbumFileReferenceMap AlbumFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "AlbumFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4811 (class 2606 OID 58281)
-- Name: Album Album_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Album"
    ADD CONSTRAINT "Album_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4827 (class 2606 OID 58261)
-- Name: ArtistFileReferenceMap ArtistFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "ArtistFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4809 (class 2606 OID 58279)
-- Name: Artist Artist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Artist"
    ADD CONSTRAINT "Artist_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4821 (class 2606 OID 58277)
-- Name: FileReference FileReference_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FileReference"
    ADD CONSTRAINT "FileReference_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4829 (class 2606 OID 58275)
-- Name: FileType FileType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FileType"
    ADD CONSTRAINT "FileType_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4813 (class 2606 OID 58273)
-- Name: Genre Genre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Genre"
    ADD CONSTRAINT "Genre_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4802 (class 2606 OID 58271)
-- Name: M3UStream M3UStream_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UStream"
    ADD CONSTRAINT "M3UStream_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4805 (class 2606 OID 58269)
-- Name: RadioBrowserStation RadioBrowserStation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RadioBrowserStation"
    ADD CONSTRAINT "RadioBrowserStation_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4831 (class 2606 OID 58265)
-- Name: TagSmallFileReferenceMap TagSmallFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallFileReferenceMap"
    ADD CONSTRAINT "TagSmallFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4823 (class 2606 OID 58267)
-- Name: TagSmallVendorMap TagSmallVendorMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallVendorMap"
    ADD CONSTRAINT "TagSmallVendorMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4817 (class 2606 OID 58263)
-- Name: TagSmall TagSmall_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmall"
    ADD CONSTRAINT "TagSmall_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4807 (class 2606 OID 58257)
-- Name: Track Track_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Track_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4819 (class 2606 OID 58283)
-- Name: Vendor Vendor_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Vendor"
    ADD CONSTRAINT "Vendor_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4803 (class 1259 OID 17220)
-- Name: NameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "NameIndex" ON public."M3UStream" USING btree ("Name") WITH (deduplicate_items='true');


--
-- TOC entry 4840 (class 2606 OID 58294)
-- Name: AlbumFileReferenceMap AlbumFileReferenceMap_Album_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "AlbumFileReferenceMap_Album_FK" FOREIGN KEY ("AlbumId") REFERENCES public."Album"("Id") NOT VALID;


--
-- TOC entry 4841 (class 2606 OID 58299)
-- Name: AlbumFileReferenceMap AlbumFileReferenceMap_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "AlbumFileReferenceMap_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4836 (class 2606 OID 58289)
-- Name: Album Album_Artist_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Album"
    ADD CONSTRAINT "Album_Artist_FK" FOREIGN KEY ("ArtistId") REFERENCES public."Artist"("Id") NOT VALID;


--
-- TOC entry 4842 (class 2606 OID 58304)
-- Name: ArtistFileReferenceMap ArtistFileReferenceMap_Artist_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "ArtistFileReferenceMap_Artist_FK" FOREIGN KEY ("ArtistId") REFERENCES public."Artist"("Id") NOT VALID;


--
-- TOC entry 4843 (class 2606 OID 58309)
-- Name: ArtistFileReferenceMap ArtistFileReferenceMap_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "ArtistFileReferenceMap_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4837 (class 2606 OID 58314)
-- Name: FileReference FileReference_FileType_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FileReference"
    ADD CONSTRAINT "FileReference_FileType_FK" FOREIGN KEY ("FileTypeId") REFERENCES public."FileType"("Id") NOT VALID;


--
-- TOC entry 4844 (class 2606 OID 58324)
-- Name: TagSmallFileReferenceMap TagSmallFileReferenceMap_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallFileReferenceMap"
    ADD CONSTRAINT "TagSmallFileReferenceMap_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4845 (class 2606 OID 58319)
-- Name: TagSmallFileReferenceMap TagSmallFileReferenceMap_TagSmall_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallFileReferenceMap"
    ADD CONSTRAINT "TagSmallFileReferenceMap_TagSmall_FK" FOREIGN KEY ("TagSmallId") REFERENCES public."TagSmall"("Id") NOT VALID;


--
-- TOC entry 4838 (class 2606 OID 58329)
-- Name: TagSmallVendorMap TagSmallVendorMap_TagSmall_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallVendorMap"
    ADD CONSTRAINT "TagSmallVendorMap_TagSmall_FK" FOREIGN KEY ("TagSmallId") REFERENCES public."TagSmall"("Id") NOT VALID;


--
-- TOC entry 4839 (class 2606 OID 58334)
-- Name: TagSmallVendorMap TagSmallVendorMap_Vendor_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallVendorMap"
    ADD CONSTRAINT "TagSmallVendorMap_Vendor_FK" FOREIGN KEY ("VendorId") REFERENCES public."Vendor"("Id") NOT VALID;


--
-- TOC entry 4832 (class 2606 OID 58344)
-- Name: Track Track_Album_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Track_Album_FK" FOREIGN KEY ("AlbumId") REFERENCES public."Album"("Id") NOT VALID;


--
-- TOC entry 4833 (class 2606 OID 58339)
-- Name: Track Track_Artist_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Track_Artist_FK" FOREIGN KEY ("ArtistId") REFERENCES public."Artist"("Id") NOT VALID;


--
-- TOC entry 4834 (class 2606 OID 58354)
-- Name: Track Track_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Track_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4835 (class 2606 OID 58349)
-- Name: Track Track_Genre_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Track_Genre_FK" FOREIGN KEY ("GenreId") REFERENCES public."Genre"("Id") NOT VALID;


-- Completed on 2026-10-10 10:47:09

--
-- PostgreSQL database dump complete
--

