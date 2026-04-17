# Memora.Api

## Purpose
Exposes Memora capabilities through a local OpenAPI-compatible service.

## Responsibilities
- project endpoints
- artifact endpoints
- approval endpoints
- context endpoints

## Does NOT contain
- duplicated core rules
- duplicated ranking logic

## Local Development
- the default development launch profile listens on `http://127.0.0.1:5081`
- set `MEMORA_WORKSPACES_ROOT` or `Memora:WorkspacesRootPath` to a workspace root before exercising file-backed endpoints
- this host is intended to run alongside `Memora.Ui`, which uses its own development port
