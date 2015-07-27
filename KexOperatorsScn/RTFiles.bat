@ECHO OFF
DEL *.suo /AH
DEL *.csproj.user
RD obj /Q /S
RD bin /Q /S
RD publish /Q /S