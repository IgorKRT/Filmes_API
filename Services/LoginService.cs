﻿using FilmesFinal.Data.Request;
using FilmesFinal.Models;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace FilmesFinal.Services
{
    public class LoginService
    {
        private SignInManager<CustomIdentityUser> _signInManager;
        private TokenService _tokenService;

        public LoginService(SignInManager<CustomIdentityUser> signInManager, TokenService tokenService)
        {
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public Result LogaUsuario(LoginRequest request)
        {
            var resultadoIdentity = _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);
            if (resultadoIdentity.Result.Succeeded) 
            { 
                var identityUser = _signInManager
                                   .UserManager
                                     .Users
                                       .FirstOrDefault(usuario => usuario.NormalizedUserName == request.Username.ToUpper());
                Token token =_tokenService.CreateToken(identityUser, _signInManager.UserManager.GetRolesAsync(identityUser).Result.FirstOrDefault());
                return Result.Ok().WithSuccess(token.Value); 
            }
            return Result.Fail("deu ruim");
        }

        public Result SoliticaResetSenhaUsuario(SolicitaResetRequest request)
        {
            CustomIdentityUser identityUser = RecuperaUsuarioPorEmail(request.Email);
            if (identityUser != null)
            {
                string codigoDeRecuperacao = _signInManager.UserManager.GeneratePasswordResetTokenAsync(identityUser).Result;
                return Result.Ok().WithSuccess(codigoDeRecuperacao);
            }
            return Result.Fail("Erro ao solicitar redefinição de senha");
        }
        public Result ResetaSenhaUsuario(EfetuaResetRequest request)
        {
            CustomIdentityUser identityUser = RecuperaUsuarioPorEmail(request.Email);
            IdentityResult resultadoIdentity = _signInManager.UserManager.ResetPasswordAsync(identityUser, request.Token, request.Password).Result;
            if(resultadoIdentity.Succeeded) { return Result.Ok().WithSuccess("Senha redifinida"); }
            return Result.Fail("Senha nao resetada.");
        }
        private CustomIdentityUser RecuperaUsuarioPorEmail(string email)
        {
            return _signInManager
                .UserManager
                .Users
                .FirstOrDefault(e => e.NormalizedEmail == email.ToUpper());
        }
    }
}
