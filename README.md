# Intune network drive mapping generator

[![.NET Core](https://github.com/nicolonsky/IntuneDriveMapping/actions/workflows/deploy-azure-appservice.yml/badge.svg?branch=master)](https://github.com/nicolonsky/IntuneDriveMapping/actions/workflows/deploy-azure-appservice.yml)

* Generate Intune PowerShell scripts to map network drives on Azure AD joined devices
* Seamlessly migrate existing network drive mapping group policies
* Generate a network drive mapping configuration from scratch
* Supports security group filtering (with nested groups)
* Supports recurring execution on clients

Documentation is available on my [blog](https://tech.nicolonsky.ch/next-level-network-drive-mapping-with-intune/) and in the [wiki](https://github.com/nicolonsky/IntuneDriveMapping/wiki#troubleshooting). The guys from Intune Training did an awesome walkthrough [video](https://youtu.be/hHtXFeuHkC4) explaining the tool and how to modify the PowerShell script for a VPN based event trigger.

![image](https://user-images.githubusercontent.com/32899754/88693062-21c4b980-d0ff-11ea-8e5e-adbc655fe0e6.png)
