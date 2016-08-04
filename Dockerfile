FROM mono:4.4.2-onbuild

VOLUME /usr/src/app/build/config
VOLUME /root/.credentials/internet-separation-adapter.json

ENTRYPOINT [ "mono",  "./InternetSeparationAdapter.exe" ]
CMD ["config/config.json"]
