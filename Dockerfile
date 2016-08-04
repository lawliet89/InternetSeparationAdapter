FROM mono:4.4.2-onbuild
ENTRYPOINT [ "mono",  "./InternetSeparationAdapter.exe" ]
