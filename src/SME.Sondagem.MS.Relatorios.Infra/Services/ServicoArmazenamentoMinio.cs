using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Infra.Services;

public class ServicoArmazenamentoMinio : IServicoArmazenamentoMinio
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public ServicoArmazenamentoMinio(IConfiguration configuration)
    {
        var endpoint = configuration["ConfiguracaoArmazenamento:Endpoint"];
        var accessKey = configuration["ConfiguracaoArmazenamento:AccessKey"];
        var secretKey = configuration["ConfiguracaoArmazenamento:SecretKey"];
        _bucketName = configuration["ConfiguracaoArmazenamento:BucketArquivos"] ?? "arquivos";

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL()
            .Build();
    }

    public async Task<string> GerarLinkDownloadAsync(string nomeArquivo, int minutosValidade = 1440)
    {
        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(nomeArquivo)
                .WithExpiry(minutosValidade * 60); // O SDK espera o tempo em segundos

            string url = await _minioClient.PresignedGetObjectAsync(args);
            return url;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao gerar link no MinIO.", ex);
        }
    }

    public async Task<string> UploadRelatorioAsync(byte[] arquivo, string nomeArquivo, string contentType)
    {
        try
        {
            var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }

            using var stream = new MemoryStream(arquivo);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(nomeArquivo)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            return $"Arquivo {nomeArquivo} enviado com sucesso para o bucket {_bucketName}.";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao gerar link no MinIO.", ex);
        }
    }
}
