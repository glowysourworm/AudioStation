--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2026-09-15 12:46:09

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
-- TOC entry 5053 (class 0 OID 0)
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
-- TOC entry 248 (class 1259 OID 52234)
-- Name: FileReference_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."FileReference" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."FileReference_Id_seq"
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);


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
-- TOC entry 5054 (class 0 OID 0)
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
-- TOC entry 5055 (class 0 OID 0)
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
-- TOC entry 247 (class 1259 OID 52219)
-- Name: TagSmallFileReferenceMap; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TagSmallFileReferenceMap" (
    "Id" integer NOT NULL,
    "TagSmallId" integer NOT NULL,
    "FileReferenceId" integer NOT NULL
);


ALTER TABLE public."TagSmallFileReferenceMap" OWNER TO postgres;

--
-- TOC entry 246 (class 1259 OID 52218)
-- Name: TagSmallFileReferenceMap_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TagSmallFileReferenceMap" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."TagSmallFileReferenceMap_Id_seq"
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);


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
-- TOC entry 5032 (class 0 OID 50663)
-- Dependencies: 233
-- Data for Name: AcoustIDLookupResult; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AcoustIDLookupResult" ("Id", "LookupId", "MusicBrainzRecordingId", "Score", "FileName") FROM stdin;
\.


--
-- TOC entry 5024 (class 0 OID 16844)
-- Dependencies: 225
-- Data for Name: Album; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Album" ("Id", "Name", "MediaNumber", "MediaCount", "Year", "MusicBrainzReleaseId", "MediaFormat") FROM stdin;
\.


--
-- TOC entry 5041 (class 0 OID 52170)
-- Dependencies: 242
-- Data for Name: AlbumFileReferenceMap; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AlbumFileReferenceMap" ("Id", "AlbumId", "FileReferenceId", "FileTypeId") FROM stdin;
\.


--
-- TOC entry 5022 (class 0 OID 16831)
-- Dependencies: 223
-- Data for Name: Artist; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Artist" ("Id", "Name", "MusicBrainzArtistId") FROM stdin;
\.


--
-- TOC entry 5042 (class 0 OID 52175)
-- Dependencies: 243
-- Data for Name: ArtistFileReferenceMap; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ArtistFileReferenceMap" ("Id", "ArtistId", "FileReferenceId", "FileTypeId") FROM stdin;
\.


--
-- TOC entry 5037 (class 0 OID 50706)
-- Dependencies: 238
-- Data for Name: FileReference; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."FileReference" ("Id", "FileName", "Created", "LastModified", "IsFileAvailable", "IsFileCorrupt", "IsFileLoadError", "FileErrorMessage", "FileCorruptMessage", "CRC32") FROM stdin;
\.


--
-- TOC entry 5043 (class 0 OID 52180)
-- Dependencies: 244
-- Data for Name: FileType; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."FileType" ("Id", "Name") FROM stdin;
4	AudioFile
5	FrontCover
6	BackCover
7	FanArt
\.


--
-- TOC entry 5026 (class 0 OID 16857)
-- Dependencies: 227
-- Data for Name: Genre; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Genre" ("Id", "Name", "MusicBrainzGenreId") FROM stdin;
\.


--
-- TOC entry 5017 (class 0 OID 16769)
-- Dependencies: 218
-- Data for Name: M3UStream; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."M3UStream" ("Id", "Duration", "Name", "GroupName", "LogoUrl", "HomepageUrl", "StreamSourceUrl", "UserExcluded") FROM stdin;
\.


--
-- TOC entry 5018 (class 0 OID 16815)
-- Dependencies: 219
-- Data for Name: RadioBrowserStation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."RadioBrowserStation" ("Id", "StationUUID", "ServerUUID", "Name", "Url", "UrlResolved", "Homepage", "Favicon", "Tags", "Country", "State", "Language", "LanguageCodes", "Codec", "Bitrate", "Hls", "UserExcluded") FROM stdin;
\.


--
-- TOC entry 5034 (class 0 OID 50669)
-- Dependencies: 235
-- Data for Name: TagSmall; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TagSmall" ("Id", "AlbumArtist", "Album", "Title", "Genre", "TrackNumber", "TrackTotal", "MediaNumber", "MediaTotal", "MediaFormat", "DurationMilliseconds", "Year") FROM stdin;
\.


--
-- TOC entry 5046 (class 0 OID 52219)
-- Dependencies: 247
-- Data for Name: TagSmallFileReferenceMap; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TagSmallFileReferenceMap" ("Id", "TagSmallId", "FileReferenceId") FROM stdin;
\.


--
-- TOC entry 5039 (class 0 OID 52115)
-- Dependencies: 240
-- Data for Name: TagSmallVendorMap; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TagSmallVendorMap" ("Id", "TagSmallId", "VendorId", "MusicBrainzRecordingId") FROM stdin;
\.


--
-- TOC entry 5020 (class 0 OID 16823)
-- Dependencies: 221
-- Data for Name: Track; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Track" ("Id", "Title", "Number", "AlbumId", "PrimaryArtistId", "DurationMilliseconds", "PrimaryGenreId", "AmazonId", "MusicBrainzTrackId", "FileReferenceId") FROM stdin;
\.


--
-- TOC entry 5028 (class 0 OID 16875)
-- Dependencies: 229
-- Data for Name: TrackArtistMap; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TrackArtistMap" ("Id", "TrackId", "ArtistId", "IsPrimaryArtist") FROM stdin;
\.


--
-- TOC entry 5030 (class 0 OID 16896)
-- Dependencies: 231
-- Data for Name: TrackGenreMap; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TrackGenreMap" ("Id", "TrackId", "GenreId", "IsPrimaryGenre") FROM stdin;
\.


--
-- TOC entry 5036 (class 0 OID 50677)
-- Dependencies: 237
-- Data for Name: Vendor; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Vendor" ("Id", "VendorName") FROM stdin;
39	AudioDB
40	Discogs
41	iTunes
42	LastFm
43	MusicBrainz
44	Spotify
\.


--
-- TOC entry 5056 (class 0 OID 0)
-- Dependencies: 232
-- Name: AcoustIDResult_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."AcoustIDResult_Id_seq"', 7891, true);


--
-- TOC entry 5057 (class 0 OID 0)
-- Dependencies: 241
-- Name: AlbumFileReferenceMap_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."AlbumFileReferenceMap_Id_seq"', 1, false);


--
-- TOC entry 5058 (class 0 OID 0)
-- Dependencies: 248
-- Name: FileReference_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."FileReference_Id_seq"', 32, true);


--
-- TOC entry 5059 (class 0 OID 0)
-- Dependencies: 245
-- Name: FileType_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."FileType_Id_seq"', 7, true);


--
-- TOC entry 5060 (class 0 OID 0)
-- Dependencies: 217
-- Name: M3UInfo_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."M3UInfo_Id_seq"', 18738138, true);


--
-- TOC entry 5061 (class 0 OID 0)
-- Dependencies: 224
-- Name: Mp3FileReferenceAlbum_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Mp3FileReferenceAlbum_Id_seq"', 25650, true);


--
-- TOC entry 5062 (class 0 OID 0)
-- Dependencies: 228
-- Name: Mp3FileReferenceArtistMap_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Mp3FileReferenceArtistMap_Id_seq"', 254014, true);


--
-- TOC entry 5063 (class 0 OID 0)
-- Dependencies: 222
-- Name: Mp3FileReferenceArtist_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Mp3FileReferenceArtist_Id_seq"', 14448, true);


--
-- TOC entry 5064 (class 0 OID 0)
-- Dependencies: 230
-- Name: Mp3FileReferenceGenreMap_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Mp3FileReferenceGenreMap_Id_seq"', 176437, true);


--
-- TOC entry 5065 (class 0 OID 0)
-- Dependencies: 226
-- Name: Mp3FileReferenceGenre_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Mp3FileReferenceGenre_Id_seq"', 5082, true);


--
-- TOC entry 5066 (class 0 OID 0)
-- Dependencies: 220
-- Name: Mp3FileReference_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Mp3FileReference_Id_seq"', 257761, true);


--
-- TOC entry 5067 (class 0 OID 0)
-- Dependencies: 246
-- Name: TagSmallFileReferenceMap_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."TagSmallFileReferenceMap_Id_seq"', 26, true);


--
-- TOC entry 5068 (class 0 OID 0)
-- Dependencies: 239
-- Name: TagSmallVendorMap_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."TagSmallVendorMap_Id_seq"', 127, true);


--
-- TOC entry 5069 (class 0 OID 0)
-- Dependencies: 234
-- Name: VendorTagSmall_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."VendorTagSmall_Id_seq"', 170, true);


--
-- TOC entry 5070 (class 0 OID 0)
-- Dependencies: 236
-- Name: VendorType_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."VendorType_Id_seq"', 44, true);


--
-- TOC entry 4838 (class 2606 OID 50667)
-- Name: AcoustIDLookupResult AcoustIDResult_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AcoustIDLookupResult"
    ADD CONSTRAINT "AcoustIDResult_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4846 (class 2606 OID 52174)
-- Name: AlbumFileReferenceMap AlbumFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "AlbumFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4848 (class 2606 OID 52179)
-- Name: ArtistFileReferenceMap ArtistFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "ArtistFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4834 (class 2606 OID 16879)
-- Name: TrackArtistMap ArtistMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackArtistMap"
    ADD CONSTRAINT "ArtistMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4844 (class 2606 OID 50712)
-- Name: FileReference FileReference_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FileReference"
    ADD CONSTRAINT "FileReference_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4850 (class 2606 OID 52186)
-- Name: FileType FileType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FileType"
    ADD CONSTRAINT "FileType_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4836 (class 2606 OID 16900)
-- Name: TrackGenreMap GenreMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackGenreMap"
    ADD CONSTRAINT "GenreMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4821 (class 2606 OID 16775)
-- Name: M3UStream M3UInfo_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."M3UStream"
    ADD CONSTRAINT "M3UInfo_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4830 (class 2606 OID 16850)
-- Name: Album Mp3FileReferenceAlbum_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Album"
    ADD CONSTRAINT "Mp3FileReferenceAlbum_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4828 (class 2606 OID 16837)
-- Name: Artist Mp3FileReferenceArtist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Artist"
    ADD CONSTRAINT "Mp3FileReferenceArtist_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4832 (class 2606 OID 16863)
-- Name: Genre Mp3FileReferenceGenre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Genre"
    ADD CONSTRAINT "Mp3FileReferenceGenre_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4826 (class 2606 OID 16829)
-- Name: Track Mp3FileReference_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Mp3FileReference_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4824 (class 2606 OID 16821)
-- Name: RadioBrowserStation RadioBrowserStation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RadioBrowserStation"
    ADD CONSTRAINT "RadioBrowserStation_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4852 (class 2606 OID 52223)
-- Name: TagSmallFileReferenceMap TagSmallFileReferenceMap_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallFileReferenceMap"
    ADD CONSTRAINT "TagSmallFileReferenceMap_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4840 (class 2606 OID 50675)
-- Name: TagSmall VendorTagSmall_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmall"
    ADD CONSTRAINT "VendorTagSmall_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4842 (class 2606 OID 50683)
-- Name: Vendor VendorType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Vendor"
    ADD CONSTRAINT "VendorType_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4822 (class 1259 OID 17220)
-- Name: NameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "NameIndex" ON public."M3UStream" USING btree ("Name") WITH (deduplicate_items='true');


--
-- TOC entry 4853 (class 2606 OID 16869)
-- Name: Track Album_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Album_ForeignKey" FOREIGN KEY ("AlbumId") REFERENCES public."Album"("Id");


--
-- TOC entry 4857 (class 2606 OID 16885)
-- Name: TrackArtistMap Artist_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackArtistMap"
    ADD CONSTRAINT "Artist_ForeignKey" FOREIGN KEY ("ArtistId") REFERENCES public."Artist"("Id");


--
-- TOC entry 4854 (class 2606 OID 16890)
-- Name: Track Artist_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Artist_ForeignKey" FOREIGN KEY ("PrimaryArtistId") REFERENCES public."Artist"("Id") NOT VALID;


--
-- TOC entry 4855 (class 2606 OID 50713)
-- Name: Track FileReference_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "FileReference_ForeignKey" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4859 (class 2606 OID 16906)
-- Name: TrackGenreMap Genre_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackGenreMap"
    ADD CONSTRAINT "Genre_ForeignKey" FOREIGN KEY ("GenreId") REFERENCES public."Genre"("Id");


--
-- TOC entry 4856 (class 2606 OID 17237)
-- Name: Track Genre_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Track"
    ADD CONSTRAINT "Genre_ForeignKey" FOREIGN KEY ("PrimaryGenreId") REFERENCES public."Genre"("Id") NOT VALID;


--
-- TOC entry 4863 (class 2606 OID 52202)
-- Name: AlbumFileReferenceMap Map_Album_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "Map_Album_FK" FOREIGN KEY ("AlbumId") REFERENCES public."Album"("Id") NOT VALID;


--
-- TOC entry 4866 (class 2606 OID 52187)
-- Name: ArtistFileReferenceMap Map_Artist_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "Map_Artist_FK" FOREIGN KEY ("ArtistId") REFERENCES public."Artist"("Id") NOT VALID;


--
-- TOC entry 4867 (class 2606 OID 52192)
-- Name: ArtistFileReferenceMap Map_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "Map_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4864 (class 2606 OID 52207)
-- Name: AlbumFileReferenceMap Map_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "Map_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4869 (class 2606 OID 52229)
-- Name: TagSmallFileReferenceMap Map_FileReference_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallFileReferenceMap"
    ADD CONSTRAINT "Map_FileReference_FK" FOREIGN KEY ("FileReferenceId") REFERENCES public."FileReference"("Id") NOT VALID;


--
-- TOC entry 4868 (class 2606 OID 52197)
-- Name: ArtistFileReferenceMap Map_FileType_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ArtistFileReferenceMap"
    ADD CONSTRAINT "Map_FileType_FK" FOREIGN KEY ("FileTypeId") REFERENCES public."FileType"("Id") NOT VALID;


--
-- TOC entry 4865 (class 2606 OID 52212)
-- Name: AlbumFileReferenceMap Map_FileType_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AlbumFileReferenceMap"
    ADD CONSTRAINT "Map_FileType_FK" FOREIGN KEY ("FileTypeId") REFERENCES public."FileType"("Id") NOT VALID;


--
-- TOC entry 4870 (class 2606 OID 52224)
-- Name: TagSmallFileReferenceMap Map_TagSmall_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallFileReferenceMap"
    ADD CONSTRAINT "Map_TagSmall_FK" FOREIGN KEY ("TagSmallId") REFERENCES public."TagSmall"("Id") NOT VALID;


--
-- TOC entry 4861 (class 2606 OID 52123)
-- Name: TagSmallVendorMap TagSmallVendorMap_TagSmall_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallVendorMap"
    ADD CONSTRAINT "TagSmallVendorMap_TagSmall_FK" FOREIGN KEY ("TagSmallId") REFERENCES public."TagSmall"("Id");


--
-- TOC entry 4862 (class 2606 OID 52118)
-- Name: TagSmallVendorMap TagSmallVendorMap_Vendor_FK; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TagSmallVendorMap"
    ADD CONSTRAINT "TagSmallVendorMap_Vendor_FK" FOREIGN KEY ("VendorId") REFERENCES public."Vendor"("Id");


--
-- TOC entry 4858 (class 2606 OID 16880)
-- Name: TrackArtistMap Track_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackArtistMap"
    ADD CONSTRAINT "Track_ForeignKey" FOREIGN KEY ("TrackId") REFERENCES public."Track"("Id");


--
-- TOC entry 4860 (class 2606 OID 16901)
-- Name: TrackGenreMap Track_ForeignKey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TrackGenreMap"
    ADD CONSTRAINT "Track_ForeignKey" FOREIGN KEY ("TrackId") REFERENCES public."Track"("Id");


-- Completed on 2026-09-15 12:46:09

--
-- PostgreSQL database dump complete
--

