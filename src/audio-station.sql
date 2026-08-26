--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2026-08-26 18:16:41

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
-- TOC entry 233 (class 1259 OID 50663)
-- Name: AcoustIDLookupResult; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AcoustIDLookupResult" (
    "Id" integer NOT NULL,
    "LookupId" uuid NOT NULL,
    "MusicBrainzRecordingId" uuid NOT NULL,
    "Score" double precision NOT NULL,
    "Fingerprint" character varying NOT NULL,
    "FileName" character varying NOT NULL
);


ALTER TABLE public."AcoustIDLookupResult" OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 50662)
-- Name: AcoustIDResult_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."AcoustIDLookupResult" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."AcoustIDResult_Id_seq"
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 225 (class 1259 OID 16844)
-- Name: Album; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Album" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL,
    "MediaNumber" integer,
    "MediaCount" integer,
    "Year" integer,
    "MusicBrainzReleaseId" uuid,
    "MediaFormat" character varying
);


ALTER TABLE public."Album" OWNER TO postgres;

--
-- TOC entry 242 (class 1259 OID 52170)
-- Name: AlbumFileReferenceMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AlbumFileReferenceMap" (
    "Id" integer NOT NULL,
    "AlbumId" integer NOT NULL,
    "FileReferenceId" integer NOT NULL,
    "FileTypeId" integer NOT NULL
);


ALTER TABLE public."AlbumFileReferenceMap" OWNER TO postgres;

--
-- TOC entry 5011 (class 0 OID 0)
-- Dependencies: 242
-- Name: TABLE "AlbumFileReferenceMap"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."AlbumFileReferenceMap" IS 'This table should track any files related to the album art. There will be a column for the type which will separate files based on their purpose for the ablum.';


--
-- TOC entry 241 (class 1259 OID 52169)
-- Name: AlbumFileReferenceMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."AlbumFileReferenceMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."AlbumFileReferenceMap_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 223 (class 1259 OID 16831)
-- Name: Artist; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Artist" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL,
    "MusicBrainzArtistId" uuid
);


ALTER TABLE public."Artist" OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 52175)
-- Name: ArtistFileReferenceMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ArtistFileReferenceMap" (
    "Id" integer NOT NULL,
    "ArtistId" integer NOT NULL,
    "FileReferenceId" integer NOT NULL,
    "FileTypeId" integer NOT NULL
);


ALTER TABLE public."ArtistFileReferenceMap" OWNER TO postgres;

--
-- TOC entry 238 (class 1259 OID 50706)
-- Name: FileReference; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."FileReference" (
    "Id" integer NOT NULL,
    "FileName" character varying NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "LastModified" timestamp with time zone NOT NULL,
    "IsFileAvailable" boolean NOT NULL,
    "IsFileCorrupt" boolean NOT NULL,
    "IsFileLoadError" boolean NOT NULL,
    "FileErrorMessage" character varying,
    "FileCorruptMessage" character varying,
    "CRC32" integer NOT NULL
);


ALTER TABLE public."FileReference" OWNER TO postgres;

--
-- TOC entry 244 (class 1259 OID 52180)
-- Name: FileType; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."FileType" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."FileType" OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 52217)
-- Name: FileType_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."FileType" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."FileType_Id_seq"
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 227 (class 1259 OID 16857)
-- Name: Genre; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Genre" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL,
    "MusicBrainzGenreId" uuid
);


ALTER TABLE public."Genre" OWNER TO postgres;

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
-- TOC entry 5012 (class 0 OID 0)
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
-- TOC entry 224 (class 1259 OID 16843)
-- Name: Mp3FileReferenceAlbum_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Album" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceAlbum_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 229 (class 1259 OID 16875)
-- Name: TrackArtistMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TrackArtistMap" (
    "Id" integer NOT NULL,
    "TrackId" integer NOT NULL,
    "ArtistId" integer NOT NULL,
    "IsPrimaryArtist" boolean NOT NULL
);


ALTER TABLE public."TrackArtistMap" OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 16874)
-- Name: Mp3FileReferenceArtistMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TrackArtistMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
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

ALTER TABLE public."Artist" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceArtist_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 231 (class 1259 OID 16896)
-- Name: TrackGenreMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TrackGenreMap" (
    "Id" integer NOT NULL,
    "TrackId" integer NOT NULL,
    "GenreId" integer NOT NULL,
    "IsPrimaryGenre" boolean NOT NULL
);


ALTER TABLE public."TrackGenreMap" OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 16895)
-- Name: Mp3FileReferenceGenreMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TrackGenreMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
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

ALTER TABLE public."Genre" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Mp3FileReferenceGenre_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 221 (class 1259 OID 16823)
-- Name: Track; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Track" (
    "Id" integer NOT NULL,
    "Title" character varying,
    "Number" integer,
    "AlbumId" integer,
    "PrimaryArtistId" integer,
    "DurationMilliseconds" integer,
    "PrimaryGenreId" integer,
    "AmazonId" character varying,
    "MusicBrainzTrackId" character varying,
    "FileReferenceId" integer NOT NULL
);


ALTER TABLE public."Track" OWNER TO postgres;

--
-- TOC entry 5013 (class 0 OID 0)
-- Dependencies: 221
-- Name: TABLE "Track"; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public."Track" IS 'Portion of an mp3 file''s data used to quickly load the library on startup. Mp3 files are also loaded at runtime to verify tag data and use / modify tags. Artwork is also loaded at runtime.';


--
-- TOC entry 220 (class 1259 OID 16822)
-- Name: Mp3FileReference_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Track" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
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
-- TOC entry 235 (class 1259 OID 50669)
-- Name: TagSmall; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TagSmall" (
    "Id" integer NOT NULL,
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
    "Year" integer
);


ALTER TABLE public."TagSmall" OWNER TO postgres;

--
-- TOC entry 240 (class 1259 OID 52115)
-- Name: TagSmallVendorMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TagSmallVendorMap" (
    "Id" integer NOT NULL,
    "TagSmallId" integer NOT NULL,
    "VendorId" integer NOT NULL,
    "MusicBrainzRecordingId" uuid
);


ALTER TABLE public."TagSmallVendorMap" OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 52114)
-- Name: TagSmallVendorMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TagSmallVendorMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."TagSmallVendorMap_Id_seq"
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 237 (class 1259 OID 50677)
-- Name: Vendor; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Vendor" (
    "Id" integer NOT NULL,
    "VendorName" character varying NOT NULL
);


ALTER TABLE public."Vendor" OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 50668)
-- Name: VendorTagSmall_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TagSmall" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."VendorTagSmall_Id_seq"
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 236 (class 1259 OID 50676)
-- Name: VendorType_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Vendor" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."VendorType_Id_seq"
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 4832 (class 2606 OID 50667)
-- Name: AcoustIDLookupResult AcoustIDResult_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AcoustIDLookupResult"
    ADD CONSTRAINT "AcoustIDResult_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4840 (class 2606 OID 52174)
-- Name: AlbumFileReferenceMap AlbumFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "AlbumFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4842 (class 2606 OID 52179)
-- Name: ArtistFileReferenceMap ArtistFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "ArtistFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4828 (class 2606 OID 16879)
-- Name: TrackArtistMap ArtistMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackArtistMap"
    ADD CONSTRAINT "ArtistMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4838 (class 2606 OID 50712)
-- Name: FileReference FileReference_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FileReference"
    ADD CONSTRAINT "FileReference_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4844 (class 2606 OID 52186)
-- Name: FileType FileType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FileType"
    ADD CONSTRAINT "FileType_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4830 (class 2606 OID 16900)
-- Name: TrackGenreMap GenreMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackGenreMap"
    ADD CONSTRAINT "GenreMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4815 (class 2606 OID 16775)
-- Name: M3UStream M3UInfo_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UStream"
    ADD CONSTRAINT "M3UInfo_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4824 (class 2606 OID 16850)
-- Name: Album Mp3FileReferenceAlbum_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Album"
    ADD CONSTRAINT "Mp3FileReferenceAlbum_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4822 (class 2606 OID 16837)
-- Name: Artist Mp3FileReferenceArtist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Artist"
    ADD CONSTRAINT "Mp3FileReferenceArtist_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4826 (class 2606 OID 16863)
-- Name: Genre Mp3FileReferenceGenre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Genre"
    ADD CONSTRAINT "Mp3FileReferenceGenre_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4820 (class 2606 OID 16829)
-- Name: Track Mp3FileReference_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Mp3FileReference_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4818 (class 2606 OID 16821)
-- Name: RadioBrowserStation RadioBrowserStation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RadioBrowserStation"
    ADD CONSTRAINT "RadioBrowserStation_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4834 (class 2606 OID 50675)
-- Name: TagSmall VendorTagSmall_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmall"
    ADD CONSTRAINT "VendorTagSmall_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4836 (class 2606 OID 50683)
-- Name: Vendor VendorType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Vendor"
    ADD CONSTRAINT "VendorType_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4816 (class 1259 OID 17220)
-- Name: NameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "NameIndex" ON public."M3UStream" USING btree ("Name") WITH (deduplicate_items='true');


--
-- TOC entry 4845 (class 2606 OID 16869)
-- Name: Track Album_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Album_ForeignKey" FOREIGN KEY ("AlbumId") REFERENCES public."Album"("Id");


--
-- TOC entry 4849 (class 2606 OID 16885)
-- Name: TrackArtistMap Artist_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackArtistMap"
    ADD CONSTRAINT "Artist_ForeignKey" FOREIGN KEY ("ArtistId") REFERENCES public."Artist"("Id");


--
-- TOC entry 4846 (class 2606 OID 16890)
-- Name: Track Artist_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Artist_ForeignKey" FOREIGN KEY ("PrimaryArtistId") REFERENCES public."Artist"("Id") NOT VALID;


--
-- TOC entry 4847 (class 2606 OID 50713)
-- Name: Track FileReference_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "FileReference_ForeignKey" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4851 (class 2606 OID 16906)
-- Name: TrackGenreMap Genre_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackGenreMap"
    ADD CONSTRAINT "Genre_ForeignKey" FOREIGN KEY ("GenreId") REFERENCES public."Genre"("Id");


--
-- TOC entry 4848 (class 2606 OID 17237)
-- Name: Track Genre_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Genre_ForeignKey" FOREIGN KEY ("PrimaryGenreId") REFERENCES public."Genre"("Id") NOT VALID;


--
-- TOC entry 4855 (class 2606 OID 52202)
-- Name: AlbumFileReferenceMap Map_Album_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "Map_Album_FK" FOREIGN KEY ("AlbumId") REFERENCES public."Album"("Id") NOT VALID;


--
-- TOC entry 4858 (class 2606 OID 52187)
-- Name: ArtistFileReferenceMap Map_Artist_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "Map_Artist_FK" FOREIGN KEY ("ArtistId") REFERENCES public."Artist"("Id") NOT VALID;


--
-- TOC entry 4859 (class 2606 OID 52192)
-- Name: ArtistFileReferenceMap Map_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "Map_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4856 (class 2606 OID 52207)
-- Name: AlbumFileReferenceMap Map_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "Map_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4860 (class 2606 OID 52197)
-- Name: ArtistFileReferenceMap Map_FileType_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "Map_FileType_FK" FOREIGN KEY ("FileTypeId") REFERENCES public."FileType"("Id") NOT VALID;


--
-- TOC entry 4857 (class 2606 OID 52212)
-- Name: AlbumFileReferenceMap Map_FileType_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "Map_FileType_FK" FOREIGN KEY ("FileTypeId") REFERENCES public."FileType"("Id") NOT VALID;


--
-- TOC entry 4853 (class 2606 OID 52123)
-- Name: TagSmallVendorMap TagSmallVendorMap_TagSmall_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallVendorMap"
    ADD CONSTRAINT "TagSmallVendorMap_TagSmall_FK" FOREIGN KEY ("TagSmallId") REFERENCES public."TagSmall"("Id");


--
-- TOC entry 4854 (class 2606 OID 52118)
-- Name: TagSmallVendorMap TagSmallVendorMap_Vendor_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallVendorMap"
    ADD CONSTRAINT "TagSmallVendorMap_Vendor_FK" FOREIGN KEY ("VendorId") REFERENCES public."Vendor"("Id");


--
-- TOC entry 4850 (class 2606 OID 16880)
-- Name: TrackArtistMap Track_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackArtistMap"
    ADD CONSTRAINT "Track_ForeignKey" FOREIGN KEY ("TrackId") REFERENCES public."Track"("Id");


--
-- TOC entry 4852 (class 2606 OID 16901)
-- Name: TrackGenreMap Track_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackGenreMap"
    ADD CONSTRAINT "Track_ForeignKey" FOREIGN KEY ("TrackId") REFERENCES public."Track"("Id");


-- Completed on 2026-08-26 18:16:41

--
-- PostgreSQL database dump complete
--

