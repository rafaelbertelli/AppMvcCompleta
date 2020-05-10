using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DevIO.App.ViewModels;
using DevIO.Business.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using DevIO.Business.Models;

namespace DevIO.App.Controllers
{
    public class ProdutosController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProdutoService _produtoService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly INotificador _notificador;

        public ProdutosController(
            IMapper mapper,
            IProdutoService produtoService,
            IProdutoRepository produtoRepository,
            IFornecedorRepository fornecedorRepository,
            INotificador notificador
        ) : base(notificador)
        {
            _mapper = mapper;
            _produtoService = produtoService;
            _produtoRepository = produtoRepository;
            _fornecedorRepository = fornecedorRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View(_mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedor()));
        }
        
        public async Task<IActionResult> Details(Guid id)
        {
            ProdutoViewModel produtoViewModel = _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoPorFornecedor(id));
            
            if (produtoViewModel == null)
                return NotFound();

            return View(produtoViewModel);
        }
         

        public async Task<IActionResult> Create()
        {
            ProdutoViewModel produtoViewModel = await PopularFornecedores(new ProdutoViewModel());

            return View(produtoViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProdutoViewModel produtoViewModel)
        {
            produtoViewModel = await PopularFornecedores(produtoViewModel);

            if (!ModelState.IsValid)
                return View(produtoViewModel);

            var imgPrefixo = Guid.NewGuid() + "_";
            
            if (! await UploadArquivo(arquivo: produtoViewModel.ImagemUpload, prefixo: imgPrefixo)) 
                return View(produtoViewModel);

            DateTime dataCadastro = DateTime.Now;
            produtoViewModel.DataCadastro = dataCadastro;
            produtoViewModel.Imagem = imgPrefixo + produtoViewModel.ImagemUpload.FileName;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            if (!OperacaoValida()) 
                return View(produtoViewModel);

            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> Edit(Guid id)
        {
            ProdutoViewModel produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null)
                return NotFound();

            return View(produtoViewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id)
                return NotFound();

            ProdutoViewModel produtoAtualizacao = await ObterProduto(id);

            if (!ModelState.IsValid) 
            {
                produtoViewModel.Fornecedor = produtoAtualizacao.Fornecedor;
                produtoViewModel.Imagem = produtoAtualizacao.Imagem;
                return View(produtoViewModel);
            }

            if (produtoViewModel.ImagemUpload != null)
            {
                var imgPrefixo = Guid.NewGuid() + "_";

                if (!await UploadArquivo(arquivo: produtoViewModel.ImagemUpload, prefixo: imgPrefixo))
                    return View(produtoViewModel);

                produtoAtualizacao.Imagem = imgPrefixo + produtoViewModel.ImagemUpload.FileName;
            }

            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            if (!OperacaoValida())
                return View(produtoViewModel);

            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Delete(Guid id)
        {
            var produtoViewModel = await _produtoRepository.ObterProdutoPorFornecedor(id);

            if (produtoViewModel == null)
                return NotFound();

            return View(_mapper.Map<ProdutoViewModel>(produtoViewModel));
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var produtoViewModel = await _produtoRepository.ObterProduto(id);

            if (produtoViewModel == null)
                return NotFound();

            await _produtoService.Remover(id);

            if (!OperacaoValida())
                return View(produtoViewModel);

            TempData["Sucesso"] = "Produto excluído com sucesso";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            ProdutoViewModel produto = _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoPorFornecedor(id));
            produto.Fornecedores = _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorRepository.ObterTodos());

            return produto;
        }

        private async Task<ProdutoViewModel> PopularFornecedores(ProdutoViewModel produto)
        {
            List<Fornecedor> dbFornecedores = new List<Fornecedor>(await _fornecedorRepository.ObterFornecedoresAtivos());

            produto.Fornecedores = _mapper.Map<IEnumerable<FornecedorViewModel>>(dbFornecedores);

            return produto;
        }

        private async Task<bool> UploadArquivo(IFormFile arquivo, string prefixo)
        {
            if (arquivo.Length <= 0) return false;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/userImages", prefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                ModelState.AddModelError(key: string.Empty, errorMessage: "Já existe um arquivo com este nome");
                return false;
            }

            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
