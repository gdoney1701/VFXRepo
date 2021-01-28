Shader "Custom/DisableZWrite"
{
	SubShader{
		Tags{

			"RenderType" = "Opaque"

		}

		Pass{
			Zwrite Off
		}
	}
}
