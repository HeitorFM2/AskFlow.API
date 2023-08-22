PGDMP         5                {            ask-flow-homologation    15.4    15.4                0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false                       0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false                       0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false                       1262    16509    ask-flow-homologation    DATABASE     �   CREATE DATABASE "ask-flow-homologation" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'Portuguese_Brazil.1252';
 '   DROP DATABASE "ask-flow-homologation";
                postgres    false            �            1259    16561 	   questions    TABLE     �   CREATE TABLE public.questions (
    id bigint NOT NULL,
    iduser integer,
    message text,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone
);
    DROP TABLE public.questions;
       public         heap    postgres    false            �            1259    16560    questions_id_seq    SEQUENCE     y   CREATE SEQUENCE public.questions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public.questions_id_seq;
       public          postgres    false    215                       0    0    questions_id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE public.questions_id_seq OWNED BY public.questions.id;
          public          postgres    false    214            �            1259    16597 	   responses    TABLE     �   CREATE TABLE public.responses (
    id bigint NOT NULL,
    idquestion integer,
    iduser integer,
    message text,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone
);
    DROP TABLE public.responses;
       public         heap    postgres    false            �            1259    16596    responses_id_seq    SEQUENCE     y   CREATE SEQUENCE public.responses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public.responses_id_seq;
       public          postgres    false    219                       0    0    responses_id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE public.responses_id_seq OWNED BY public.responses.id;
          public          postgres    false    218            �            1259    16571    users    TABLE       CREATE TABLE public.users (
    id bigint NOT NULL,
    first_name text,
    last_name text,
    email text,
    img text,
    password text,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone
);
    DROP TABLE public.users;
       public         heap    postgres    false            �            1259    16570    users_id_seq    SEQUENCE     u   CREATE SEQUENCE public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 #   DROP SEQUENCE public.users_id_seq;
       public          postgres    false    217                       0    0    users_id_seq    SEQUENCE OWNED BY     =   ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;
          public          postgres    false    216            o           2604    16564    questions id    DEFAULT     l   ALTER TABLE ONLY public.questions ALTER COLUMN id SET DEFAULT nextval('public.questions_id_seq'::regclass);
 ;   ALTER TABLE public.questions ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    215    214    215            q           2604    16600    responses id    DEFAULT     l   ALTER TABLE ONLY public.responses ALTER COLUMN id SET DEFAULT nextval('public.responses_id_seq'::regclass);
 ;   ALTER TABLE public.responses ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    219    218    219            p           2604    16574    users id    DEFAULT     d   ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);
 7   ALTER TABLE public.users ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    217    216    217                      0    16561 	   questions 
   TABLE DATA           \   COPY public.questions (id, iduser, message, created_at, updated_at, deleted_at) FROM stdin;
    public          postgres    false    215   �       	          0    16597 	   responses 
   TABLE DATA           h   COPY public.responses (id, idquestion, iduser, message, created_at, updated_at, deleted_at) FROM stdin;
    public          postgres    false    219   G                 0    16571    users 
   TABLE DATA           t   COPY public.users (id, first_name, last_name, email, img, password, created_at, updated_at, deleted_at) FROM stdin;
    public          postgres    false    217   �                  0    0    questions_id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public.questions_id_seq', 20, true);
          public          postgres    false    214                       0    0    responses_id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public.responses_id_seq', 6, true);
          public          postgres    false    218                       0    0    users_id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('public.users_id_seq', 3, true);
          public          postgres    false    216            s           2606    16568    questions questions_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY public.questions
    ADD CONSTRAINT questions_pkey PRIMARY KEY (id);
 B   ALTER TABLE ONLY public.questions DROP CONSTRAINT questions_pkey;
       public            postgres    false    215            u           2606    16578    users users_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);
 :   ALTER TABLE ONLY public.users DROP CONSTRAINT users_pkey;
       public            postgres    false    217               �  x����N#1���S�ى�y��^{��� ��?�v�����9����wޟ���������vr9B?f8`D�P��;��/����*��<~�\b�
I:V�|��OK�}2���Ire��_.�@�Q�B�������-�m�<�$�"GUԾD�W!��"Uʬ�TU�q&�A��$�YUQ�e�0cp U#��ͥ$�po���oTM�]ʨ]ML�)h���Bu��_s��q�)�5��~�/�A��S�B4�Sӕg�:��D �p:hڕp~��iI
Rz婙�b^�j�
�:�<5��|.�9�����),�����%e�Y�C�X�/�ht��{t#GM$[��V��=zn�ڿ��}ϗ��C���=�#0:��S������7�      	   u   x�3�4�ĤҤ�T�?�2�4�"jV[�.l(��M�,�W(�/.�4202�5��52T02�25�2��30�4�4�50& 4Ҕ��&V�zfFf&X�D�i4���1z\\\ ��:�         /  x�}�Ko�@���+�p�80�Z��
>�-q�dD@D���%m�4�tsnn��|"0YR�؁�a�������k���FE@�=���c$����߈5�G^�P�{�q�*��5i<%�gt�-��z��K}D���T�e#(!�HI������]��O�mԬ�YM��23��Su��ef/�����q��/J�
|uq�hi�o���wc����*ړ��mq��-O�����&[Vu��q9��|���3%�ډm�|�=�7^z������#���#"�`�v��V��� < ���`     